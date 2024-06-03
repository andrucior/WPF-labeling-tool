using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wpf2
{
    public partial class MenuWindow : Window
    {
        public ObservableCollection<MyImage> Images { get; set; }
        public ObservableCollection<MyImage> ChosenImages { get; set; }
        public MyImage SelectedImage { get; set; }

        public MenuWindow()
        {
            InitializeComponent();
            Images = new ObservableCollection<MyImage>();
            ChosenImages = new ObservableCollection<MyImage>();
            DataContext = this;

            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\resources\\uploaded_images"))
            {
                DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\resources\\uploaded_images");
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\resources\\uploaded_images");
        }

        private void ClickMenu(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(ChosenImages);
            mainWindow.Show();
            mainWindow.Visibility = Visibility.Visible;
            this.Close();
        }

        private void LoadDatasetClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\users";

            if (dlg.ShowDialog() == true)
            {
                var info = new DirectoryInfo(dlg.ResultPath);
                FileInfo[] imageFiles = info.GetFiles("*.*", SearchOption.AllDirectories)
                                                         .Where(file => file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                        file.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                        file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                                                         .ToArray();

                foreach (var file in imageFiles)
                {
                    string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources\\uploaded_images", file.Name);
                    File.Copy(file.FullName, destFilePath, true);

                    MyImage image = new MyImage()
                    {
                        Name = file.Name,
                        Uri = destFilePath,
                        Origin = file.FullName,
                    };
                    Images.Add(image);
                }
            }
        }

        private void moveImage(object sender, RoutedEventArgs e)
        {
            foreach (var im in ImageList.SelectedItems)
            {
                var img = im as MyImage;
                ChosenImages.Add(img);
            }
            if (ChosenImages.Count > 0) StartLabelingButton.IsEnabled = true;
            else StartLabelingButton.IsEnabled = false;
        }

        private void deleteImage(object sender, RoutedEventArgs e)
        {
            var img = ImageList.SelectedItem as MyImage;
            ChosenImages.Remove(img);
        }

        private void ChangePreview(object sender, RoutedEventArgs e)
        {
            var uri = sender as Button;
            PreviewImage.Source = new BitmapImage(new Uri((string)uri.Content));
        }

        private void StartLabeling(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(ChosenImages);
            mainWindow.Show();
        }
    }

    public class MyImage
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Origin { get; set; }
    }
}
