using Soundlyzer.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;

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
