using System;
using System.Windows;
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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace Soundlyzer
{
	public class AudioFileViewModel : INotifyPropertyChanged
	{
		private double _progress;
        private string _status;
        private bool _isProcessing;
        private bool _isPaused;
        private ManualResetEventSlim _pauseEvent = new(true);

		public string FileName => Path.GetFileName(FilePath);
		public string FilePath { get; set; }
        public float[] Samples { get; set; }
        public Complex[][] Spectrogram { get; set; }
        public int SampleRate { get; set; }
        public CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();

        public ICommand StartCommand { get; set; }
        public ICommand CancelCommand { get; set; }
		public ICommand PauseResumeCommand { get; set; }
		public ICommand OpenCommand { get; set; }
        public ICommand StartAllCommand { get; set; }


        public ObservableCollection<AudioFileViewModel> AudioFiles { get; set; }

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
        private async Task StartAllProcessing()
        {
            foreach (var audioFile in AudioFiles)
            {
                if (!audioFile.IsProcessing)
                {
                    await audioFile.StartProcessing();
                }
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
				(Samples, SampleRate) = await Task.Run(() =>
				{
					var samples = AudioProcessor.ReadSamplesMono(FilePath, out int sampleRate);
					return (samples, sampleRate);
				}, Cts.Token);

				Spectrogram = await CalculateSpectrogramWithPause(Samples, SampleRate, Cts.Token);
				SaveSpectrogramAsImage();
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
				if (Status == "done") Progress = 100;
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
			Progress = 0;

            StartCommand = new RelayCommand(async () => await StartProcessing());
            CancelCommand = new RelayCommand(Cancel);
			PauseResumeCommand = new RelayCommand(TogglePause);
			OpenCommand = new RelayCommand(OpenSpectrogram);
            StartAllCommand = new RelayCommand(async () => await StartAllProcessing());
        }
       
        //obliczanie spektrogramu
        private async Task<Complex[][]> CalculateSpectrogramWithPause(
			float[] samples, int sampleRate, CancellationToken token,
			int windowSize = 1024, int overlap = 512)
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
				await Task.Delay(1); // pozwala UI odświeżać pasek postępu
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
		private void SaveSpectrogramAsImage()
		{
			if (Spectrogram == null || Spectrogram.Length == 0)
				return;

			int width = Spectrogram.Length;
			int height = Spectrogram[0].Length;

			var bitmap = new System.Windows.Media.Imaging.WriteableBitmap(
				width, height, 96, 96, PixelFormats.Gray8, null);

			byte[] pixels = new byte[width * height];

			double maxMagnitude = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double mag = Spectrogram[x][y].Magnitude;
					double db = 20 * Math.Log10(mag + 1e-12); 
					maxMagnitude = Math.Max(maxMagnitude, db);
				}
			}

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double mag = Spectrogram[x][y].Magnitude;
					double db = 20 * Math.Log10(mag + 1e-12);
					byte intensity = (byte)(Math.Clamp(db / maxMagnitude, 0, 1) * 255);

					int pixelIndex = (height - y - 1) * width + x; 
					pixels[pixelIndex] = intensity;
				}
			}

			bitmap.WritePixels(
				new Int32Rect(0, 0, width, height),
				pixels, width, 0);

			string outputPath = Path.ChangeExtension(FilePath, ".png");

			using (var fileStream = new FileStream(outputPath, FileMode.Create))
			{
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(bitmap));
				encoder.Save(fileStream);
			}
		}

		private void OpenSpectrogram()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					var viewer = new Soundlyzer.View.ImageViewerWindow(FilePath);
					viewer.Show();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Nie udało się otworzyć obrazu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
		}
        
    }
}
