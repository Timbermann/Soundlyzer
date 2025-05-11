using Soundlyzer.Model;
using System.Collections.ObjectModel;

namespace Soundlyzer.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<File> Files { get; set; }

        public MainViewModel()
        {
            Files = new ObservableCollection<File>
            {
                new File("1.mp3"),
                new File("2.mp3"),
                new File("3.mp3"),

            };
        }
    }
}
