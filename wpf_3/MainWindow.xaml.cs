using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpf2
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<LabelItem> labels;
        private Point? StartPoint;
        private Rectangle Rect;
        private Brush Selected;
        private LabelItem LabelWithMenu;
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
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            Brush brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(1, 255),
                  (byte)rand.Next(1, 255), (byte)rand.Next(1, 233)));

            var labelItem = new LabelItem
            {
                Content = Textbox.Text,
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
            var button = sender as Button;
            Selected = button.Background;
        }

        private void ShowContextMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: do poprawy

            if (!(sender is FrameworkElement element) || !(element.DataContext is LabelItem item))
                return;

            var template = FindResource("MenuTemplate") as ContextMenu;
            if (template == null) return;
            template.PlacementTarget = element;
            template.DataContext = item;
            template.IsOpen = true;
            LabelWithMenu = sender as LabelItem;
        }

        private void EditCommand(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveCommand(object sender, RoutedEventArgs e)
        {
            if (LabelWithMenu != null)
            {
                Labels.Remove(LabelWithMenu); 
                LabelWithMenu = null; 
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
                Label label1 = new Label
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
}
