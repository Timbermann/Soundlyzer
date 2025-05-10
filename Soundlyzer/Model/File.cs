using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Soundlyzer.View;
using System.Windows;
using System.Windows.Input;

namespace Soundlyzer.Model
{

    public class File : INotifyPropertyChanged
    {
        public string FileName { get; set; }

        private string status;
        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        private double progress;
        public double Progress
        {
            get => progress;
            set { progress = value; OnPropertyChanged(); }
        }

        private bool isPaused;

        public string PauseResumeLabel => isPaused ? "Wznów" : "Pauza";

        public ICommand StartCommand { get; }
        public ICommand PauseResumeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand OpenCommand { get; }

        public File(string fileName)
        {
            FileName = fileName;
            Status = "Oczekuje";
            Progress = 0;

            StartCommand = new RelayCommand(Start);
            PauseResumeCommand = new RelayCommand(PauseResume);
            CancelCommand = new RelayCommand(Cancel);
            OpenCommand = new RelayCommand(Open);
        }

        private void Start()
        {
            Status = "Rozpoczęto";
            // symuluj start konwersji
        }

        private void PauseResume()
        {
            isPaused = !isPaused;
            Status = isPaused ? "Wstrzymano" : "Wznowiono";
            OnPropertyChanged(nameof(PauseResumeLabel));
        }

        private void Cancel()
        {
            Status = "Anulowano";
            Progress = 0;
        }

        private void Open()
        {
            try
            {
                // otwiranie okna niemodalnego
                var viewer = new ImageViewerWindow(FileName);
                viewer.Show(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd otwierania obrazu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
