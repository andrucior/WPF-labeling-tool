using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
        public MyImage ChosenSelectedImage { get; set; }
        private MyImage ParentFolder { get; set; }
        private MyImage CurrentFolder { get; set; }

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
                ParentFolder = new MyImage()
                {
                    Name = "..",
                    Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                    Origin = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png")
                };

                CurrentFolder = new MyImage()
                {
                    Name = dlg.ResultPath,
                    Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                    Origin = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png")
                };
                
                Images.Add(ParentFolder);
                Images.Add(CurrentFolder);
                
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

            var img = ImageList.SelectedItem as MyImage;
            ChosenImages.Add(img);
            Images.Remove(img);

            if (ChosenImages.Count > 0)
                StartLabelingButton.IsEnabled = true;
           
            StartLabelingMenu.IsEnabled = StartLabelingButton.IsEnabled;
        }

        private void deleteImage(object sender, RoutedEventArgs e)
        {
            var toMove = ChosenList.SelectedItem as MyImage;
            ChosenImages.Remove(toMove);
            Images.Add(toMove);

            if (ChosenImages.Count == 0)
            {
                StartLabelingButton.IsEnabled = false;
                PreviewImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "resources\\no_preview_image.png")));
            }
            else
            {
                ChosenList.SelectedItem = ChosenImages[0];
                PreviewImage.Source = new BitmapImage(new Uri(ChosenImages[0].Uri));
            }
            StartLabelingMenu.IsEnabled = StartLabelingButton.IsEnabled;
            
        }
        

        private void ChangePreview(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string check = Convert.ToString(button.Content);
            foreach (var img in ChosenImages)
            {
                if (img.Origin == Convert.ToString(button.Content))
                    ChosenList.SelectedItem = img;
            }
            
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
