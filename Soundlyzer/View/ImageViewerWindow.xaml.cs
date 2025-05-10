using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Soundlyzer.View
{
    public partial class ImageViewerWindow : Window
    {
        public ImageViewerWindow(string filePath)
        {
            InitializeComponent();

            // Zmień rozszerzenie na .png
            string imagePath = Path.ChangeExtension(filePath, ".png");

            if (File.Exists(imagePath))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                ImageDisplay.Source = bitmap;
            }
            else
            {
                MessageBox.Show($"Nie znaleziono obrazu: {imagePath}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close(); 
            }
        }
    }
}
