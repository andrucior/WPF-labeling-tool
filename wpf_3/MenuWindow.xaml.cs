using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
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
        private void MoveToParent()
        {
            try
            {
                var di = new DirectoryInfo(ParentFolder.Origin).Parent;
                if (di != null)
                {
                    Images.Clear();
                    CurrentFolder.Origin = ParentFolder.Origin;
                    CurrentFolder.Name = ParentFolder.Origin;
                    ParentFolder.Origin = di.FullName;

                    Images.Add(ParentFolder);
                    // Images.Add(CurrentFolder);

                    var info = new DirectoryInfo(CurrentFolder.Origin);
                    foreach (var dir in info.EnumerateDirectories())
                    {
                        var dir_im = new MyImage()
                        {
                            Name = dir.Name,
                            Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                            Origin = dir.FullName
                        };
                        Images.Add(dir_im);
                    }
                    FileInfo[] imageFiles = info.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                             .Where(file => file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                            file.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                            file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                                                             .ToArray();
                    foreach (var file in imageFiles)
                    {
                        string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources\\uploaded_images", file.Name);

                        MyImage image = new MyImage()
                        {
                            Name = file.Name,
                            Uri = file.FullName,
                            Origin = file.FullName,
                        };
                        Images.Add(image);
                    }
                }
            }
            catch (SecurityException)
            { MessageBox.Show("You don't have access to this directory!"); }
            

        }
        private void LoadDatasetClick(object sender, RoutedEventArgs e)
        {
            Images.Clear();
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\users";

            if (dlg.ShowDialog() == true)
            {
                var di = new DirectoryInfo(dlg.ResultPath).Parent;
                var info = new DirectoryInfo(dlg.ResultPath);

                if (di != null)
                {
                    ParentFolder = new MyImage()
                    {
                        Name = "..",
                        Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                        Origin = di.FullName
                    };
                    CurrentFolder = new MyImage()
                    {
                        Name = di.Name,
                        Origin = di.FullName
                    };
                    Images.Add(ParentFolder);
                }
                
                foreach(var dir in info.EnumerateDirectories())
                {
                    var dir_im = new MyImage()
                    {
                        Name = dir.Name,
                        Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                        Origin = dir.FullName
                    };
                    Images.Add(dir_im);
                }

                FileInfo[] imageFiles = info.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                         .Where(file => file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                        file.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                        file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                                                         .ToArray();      
                foreach (var file in imageFiles)
                {
                    // string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources\\uploaded_images", file.Name);
                    // File.Copy(file.FullName, destFilePath, true);

                    MyImage image = new MyImage()
                    {
                        Name = file.Name,
                        Uri = file.FullName,
                        Origin = file.FullName,
                    };
                    Images.Add(image);
                }
            }
        }

        private void moveImage(object sender, RoutedEventArgs e)
        {
            var img = ImageList.SelectedItem as MyImage;
            
            if (img == null) return;
            if (img.Name == ".." || Directory.Exists(img.Origin)) return;
            ChosenImages.Add(img);
            Images.Remove(img);

            if (ChosenImages.Count > 0)
                StartLabelingButton.IsEnabled = true;
           
            StartLabelingMenu.IsEnabled = StartLabelingButton.IsEnabled;
        }

        private void deleteImage(object sender, RoutedEventArgs e)
        {
            var toMove = ChosenList.SelectedItem as MyImage;
            if (toMove == null) return;

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
            if (button == null) return;

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

        private void ChangeFolder(object sender, MouseButtonEventArgs e)
        {
            var img = ImageList.SelectedItem as MyImage;
            if (img == null) return;

            if (img.Name == "..")
                MoveToParent();
            else if (Directory.Exists(img.Origin))
                MoveToChild(img.Origin);

        }
        private void MoveToChild(string dir_name)
        {
            Images.Clear();
            var di = new DirectoryInfo(dir_name).Parent;
            var info = new DirectoryInfo(dir_name);

            if (di != null)
            {
                ParentFolder = new MyImage()
                {
                    Name = "..",
                    Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                    Origin = di.FullName
                };
                CurrentFolder = new MyImage()
                {
                    Name = dir_name,
                    Origin = dir_name,
                };
                Images.Add(ParentFolder);
            }

            foreach (var dir in info.EnumerateDirectories())
            {
                var dir_im = new MyImage()
                {
                    Name = dir.Name,
                    Uri = Path.Combine(Directory.GetCurrentDirectory(), "resources\\folder_icon.png"),
                    Origin = dir.FullName
                };
                Images.Add(dir_im);
            }

            FileInfo[] imageFiles = info.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                     .Where(file => file.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                    file.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                    file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                                                     .ToArray();

            foreach (var file in imageFiles)
            {
                // string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "resources\\uploaded_images", file.Name);
                // File.Copy(file.FullName, destFilePath, true);

                MyImage image = new MyImage()
                {
                    Name = file.Name,
                    Uri = file.FullName,
                    Origin = file.FullName,
                };
                Images.Add(image);
            }
        }
    }

    public class MyImage
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Origin { get; set; }
    }
}
