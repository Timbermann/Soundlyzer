using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Numerics;
using System.IO;

namespace Soundlyzer
{
    public class AudioFileViewModel
    {
        private double _progress;
        private string _status;
        private bool _isProcessing;

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
        private async Task StartProcessing()
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Status = "processing...";
            Cts = new CancellationTokenSource();
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
            StartCommand = new RelayCommand(async _ => await StartProcessing());
            CancelCommand = new RelayCommand(_ => Cancel());
        }
    } }
