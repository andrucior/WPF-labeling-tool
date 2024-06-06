using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Label = System.Windows.Controls.Label;

namespace wpf2
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // TODO:
        // FINISH_CLICK
        // POPRAWA MENU ETYKIETY
        // 
        private ObservableCollection<LabelItem> labels;
        private Point? StartPoint;
        private Rectangle? Rect;
        private Brush? Selected;
        private string? SelectedLabel;
        private LabelItem? LabelWithMenu;
        private string ChosenDir;
        private TextBox? SelectedButton;
        private MyImage? SelectedImage;
        private Dictionary<(CroppedBitmap bitmap, string label), MyImage> CroppedBitmaps { get; set; }
        private Dictionary<MyImage, List<(Rectangle, LabelItem)>> RectanglesOfImage { get; set; }
        public ObservableCollection<MyImage> Images { get; set; }
        public ObservableCollection<LabelItem> Labels
        {
            get { return labels; }
            set
            {
                labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Labels = new ObservableCollection<LabelItem>();
            Images = new ObservableCollection<MyImage>();
            ChosenDir = string.Empty;
            CroppedBitmaps = new Dictionary<(CroppedBitmap bitmap, string label), MyImage>();
            DataContext = this;
        }
        public MainWindow(ObservableCollection<MyImage> images)
        {
            InitializeComponent();
            Labels = new ObservableCollection<LabelItem>();
            ChosenDir = string.Empty;
            CroppedBitmaps = new Dictionary<(CroppedBitmap bitmap, string label), MyImage>();
            RectanglesOfImage = new Dictionary<MyImage, List<(Rectangle, LabelItem)>> ();
            DataContext = this;
            Images = images;
            foreach (var image in Images) 
            {
                RectanglesOfImage.Add(image, new List<(Rectangle, LabelItem)>());
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            Brush brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(1, 255),
                  (byte)rand.Next(1, 255), (byte)rand.Next(1, 233)));

            var labelItem = new LabelItem
            {
                Content = MyTextbox.Text,
                Background = brush
            };
            Labels.Add(labelItem);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MouseDownOnIm(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartPoint = e.GetPosition(DrawingCanvas);
            if (Selected == null)
            {
                MessageBox.Show("Choose color first!");
                return;
            }

            Rect = new Rectangle
            {
                Stroke = Selected,
                StrokeThickness = 2,
                Fill = new SolidColorBrush() { Opacity = 0 }
            };
            Canvas.SetLeft(Rect, StartPoint.Value.X);
            Canvas.SetTop(Rect, StartPoint.Value.Y);
            DrawingCanvas.Children.Add(Rect);
        }

        private void drawRect(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)DrawingCanvas.RenderSize.Width,
            (int)DrawingCanvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(DrawingCanvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect((int)Canvas.GetLeft(Rect), (int)Canvas.GetTop(Rect), (int)Rect.Width, (int)Rect.Height));
            CroppedBitmaps.Add((crop, SelectedLabel), SelectedImage);
            foreach(var item in labels)
            {
                if (item.Content == SelectedLabel)
                {
                    RectanglesOfImage[SelectedImage].Add((Rect, item));
                    break;
                }
            }
            
            Rect = null;
        }

        private void MouseMoveOnIm(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Rect == null) return;

            Point pos = e.GetPosition(DrawingCanvas);

            double x = Math.Min(StartPoint.Value.X, pos.X);
            double y = Math.Min(StartPoint.Value.Y, pos.Y);
            double width = Math.Abs(StartPoint.Value.X - pos.X);
            double height = Math.Abs(StartPoint.Value.Y - pos.Y);

            Rect.Width = width;
            Rect.Height = height;
            Canvas.SetLeft(Rect, x);
            Canvas.SetTop(Rect, y);
        }


        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img != null)
            {
                var bitmap = img.Source as BitmapImage;
                if (bitmap != null)
                {
                    DrawingCanvas.Width = bitmap.PixelWidth;
                    DrawingCanvas.Height = bitmap.PixelHeight;
                }
            }
        }

        private void ColorSelect(object sender, RoutedEventArgs e)
        {
            var button = sender as TextBox;
            Selected = button.Background;
            SelectedLabel = button.Text;
        }

        private void ShowContextMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: do poprawy
            ContextMenu = (ContextMenu)Resources["MenuTemplate"];
            var tmp = (TextBox)sender;
            SelectedButton = tmp;
            foreach (var label in Labels)
            {
                if (tmp.Text == label.Content)
                {
                    LabelWithMenu = label;
                    break;
                }    
            }
        }

        private void EditCommand(object sender, RoutedEventArgs e)
        {
            SelectedButton.IsReadOnly = false; 
        }
        private void EnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedButton.IsReadOnly = true;
                SelectedButton.IsReadOnlyCaretVisible = false;
            }
        }
        
        private void RemoveCommand(object sender, RoutedEventArgs e)
        {
            if (LabelWithMenu != null)
            {
                Labels.Remove(LabelWithMenu);
                LabelWithMenu = null;
            }
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            int i = ImageList.SelectedIndex;
            if (i == -1) return;
            ImageList.SelectedIndex = i - 1;
        
        }

        private void FinishClick(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)DrawingCanvas.RenderSize.Width,
            (int)DrawingCanvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(DrawingCanvas);

            int i = 0;
            string path;
            foreach (var img in CroppedBitmaps)
            {
                
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(img.Key.bitmap));

                if (!Directory.Exists(PathTextBox.Text))
                    Directory.CreateDirectory(PathTextBox.Text);
                do
                {
                    path = System.IO.Path.Combine(PathTextBox.Text, img.Key.label + $"_{i++}.png");
                } while (File.Exists(path));

                using (var fs = System.IO.File.OpenWrite(path))
                {
                    pngEncoder.Save(fs);
                }

            }
            this.Close();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            int i = ImageList.SelectedIndex;
            if (i == -1) return;
            ImageList.SelectedIndex = i + 1;
        }

        private void EnableDisable(object sender, SelectionChangedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
            DrawingCanvas.Children.Add(ImageOnCanvas);
            SelectedImage = Images[ImageList.SelectedIndex];
            var tmp = RectanglesOfImage[SelectedImage];

            foreach (var rect in tmp)
            {
                DrawingCanvas.Children.Add(rect.Item1);

            }
            if (ImageList.Items.Count == 1) return;

            if (ImageList.SelectedIndex <= 0) PrevButton.IsEnabled = false;
            else PrevButton.IsEnabled = true;

            if (ImageList.SelectedIndex == Images.Count - 1) NextButton.IsEnabled = false;
            else NextButton.IsEnabled = true;
        }

        private void ChooseFolderClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\users";

            if (dlg.ShowDialog() == true)
            {
                ChosenDir = dlg.ResultPath;
                PathTextBox.Text = ChosenDir;
                FinishButton.IsEnabled = true;
            }

        }
    }

    public class LabelItem
    {
        public string Content { get; set; }
        public Brush Background { get; set; }
    }

    public class LabelConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LabelItem label)
            {
                System.Windows.Controls.Label label1 = new Label
                {
                    Content = label.Content,
                    Background = label.Background
                };
                return label1;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ImgToUri : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is MyImage tmp)
            {
                return tmp.Uri;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InvalidPathRule : ValidationRule
    {
        public override ValidationResult Validate(object value,
        CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult(false, "Path cannot be empty.");

            if (!Directory.Exists(value.ToString()))
                return new ValidationResult(false, "Given directory does not exist");
            return ValidationResult.ValidResult;
        }
    }
}
    
