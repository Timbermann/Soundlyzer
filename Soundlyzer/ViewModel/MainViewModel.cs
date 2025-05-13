using Soundlyzer.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Soundlyzer.ViewModel
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<AudioFileViewModel> Files { get; set; } = new ObservableCollection<AudioFileViewModel>();

		public ICommand AddFilesCommand { get; }
        public ICommand StartAllCommand { get; }


        public MainViewModel()
		{
			AddFilesCommand = new RelayCommand(AddFiles);
			StartAllCommand = new RelayCommand(async () => await StartAllProcessing());
		}
		private async Task StartAllProcessing()
		{
			var tasks = new List<Task>();

			foreach (var file in Files)
			{
				if (!file.IsProcessing)
					tasks.Add(file.StartProcessing()); 
			}

			Task.WhenAll(tasks).ContinueWith(t =>
			{
				if (t.IsCompletedSuccessfully)
					MessageBox.Show("Wszystkie pliki zostały przetworzone.");
				else if (t.IsFaulted)
					MessageBox.Show("Wystąpił błąd podczas przetwarzania.");

			}, TaskScheduler.FromCurrentSynchronizationContext()); 

		}

		private void AddFiles()
		{
			var dialog = new OpenFileDialog
			{
				Multiselect = true,
				Filter = "Pliki audio (*.mp3;*.wav)|*.mp3;*.wav|Wszystkie pliki (*.*)|*.*"
			};

			if (dialog.ShowDialog() == true)
			{
				foreach (var filePath in dialog.FileNames)
				{
					Files.Add(new AudioFileViewModel(filePath));
				}
			}
		}

		// Implementacja INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
