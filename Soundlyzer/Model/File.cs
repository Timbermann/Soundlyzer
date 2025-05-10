using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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


        public File(string fileName)
        {
            FileName = fileName;
            Status = "Oczekuje";
            Progress = 0;

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
            Status = "Otwarto";
            // logika otwarcia pliku (np. Process.Start)
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
