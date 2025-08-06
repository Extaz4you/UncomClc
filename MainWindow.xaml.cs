using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UncomClc.ViewModels;

namespace UncomClc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void InfoButton(object sender, RoutedEventArgs e)
        {
            Window imageWindow = new Window
            {
                Title = "Информация по выбору типа кабеля",
                Width = 800,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/infobutton.jpg"));

            imageWindow.Content = image;
            imageWindow.ShowDialog();
        }
    }
}
