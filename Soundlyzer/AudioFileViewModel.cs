using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Numerics;
using System.IO;
using Soundlyzer.Model;
using Soundlyzer.ViewModel;
using Soundlyzer.View;

namespace Soundlyzer
{
    public class AudioFileViewModel
    {
        private double _progress;
        private string _status;
        private bool _isProcessing;
        private bool _isPaused;
        private ManualResetEventSlim _pauseEvent = new(true);

        public string FilePath { get; set; }
        public float[] Samples { get; set; }
        public Complex[][] Spectrogram { get; set; }
        public int SampleRate { get; set; }
        public CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();

        public ICommand StartCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand PauseCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
            }
        }
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }
        private async Task StartProcessing()
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Status = "processing...";
            Cts = new CancellationTokenSource();
            _pauseEvent.Set();
            try
            {
                await Task.Run(() =>
                {
                    Samples = AudioProcessor.ReadSamplesMono(FilePath, out int sampleRate);
                    SampleRate = sampleRate;
                    Spectrogram = AudioProcessor.CalculateSpectrogram(Samples, sampleRate);
                }, Cts.Token);
                Status = "done";
            }

            catch (OperationCanceledException)
            {
                Status = "canceled";
            }
            catch (Exception ex)
            {
                Status = $"error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                Progress = 100;
            }
            
        }
        private void Cancel()
        {
            if (IsProcessing)
            {
                Cts.Cancel();
                Status = "cancelling...";
            }
        }

        public AudioFileViewModel(string path)
        {
            FilePath = path;
            Status = "ready";
            Samples = Array.Empty<float>();
            Spectrogram = Array.Empty<Complex[]>();
            StartCommand = new RelayCommand(async () => await StartProcessing());
            CancelCommand = new RelayCommand(Cancel);
            PauseCommand = new RelayCommand(TogglePause);
        }
        //obliczanie spektrogramu
        private Complex[][] CalculateSpectrogramWithPause(float[] samples, int sampleRate, CancellationToken token, int windowSize = 1024, int overlap = 512)
        {
            int stride = windowSize - overlap;
            int segments = (samples.Length - windowSize) / stride;
            var result = new Complex[segments][];

            for (int i = 0; i < segments; i++)
            {
                token.ThrowIfCancellationRequested();
                _pauseEvent.Wait(token);

                Complex[] buffer = new Complex[windowSize];
                for (int j = 0; j < windowSize; j++)
                    buffer[j] = samples[i * stride + j];

                MathNet.Numerics.IntegralTransforms.Fourier.Forward(buffer, MathNet.Numerics.IntegralTransforms.FourierOptions.Matlab);
                result[i] = buffer;
                Progress = (double)i / segments * 100;
            }

            return result;
        }
        private void TogglePause()
        {
            if (IsPaused)
            {
                _pauseEvent.Set();
                IsPaused = false;
                Status = "resumed";
            }
            else
            {
                _pauseEvent.Reset();
                IsPaused = true;
                Status = "paused";
            }
        }
    } }
