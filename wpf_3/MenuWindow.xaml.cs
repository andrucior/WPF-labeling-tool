using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace wpf2
{
    public partial class MenuWindow : Window
    {
        public ObservableCollection<MyImage> Images { get; set; }
        public MyImage SelectedImage { get; set; }

        public MenuWindow()
        {
            InitializeComponent();
            Images = new ObservableCollection<MyImage>();
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
            MainWindow mainWindow = new MainWindow();
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
                        Uri = destFilePath
                    };
                    Images.Add(image);
                }
            }
        }
    }

    public class MyImage
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}
