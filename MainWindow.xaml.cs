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
using UncomClc.Views;
using UncomClc.Views.InsulationsView;

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
            Load();
        }
        private void Load()
        {
            var config = Data.UploadedData.Instance;
            ClearComboBoxes();

            AddItemsToComboBox(Pipes, config.Pipes.Select(pipe => pipe.Name));
            AddItemsToComboBox(ThermalIsolationCombobox, config.Insulations.Select(ins => ins.Name));
            AddItemsToComboBox(ThermalIsolationCombobox2, config.Insulations.Select(ins => ins.Name));

        }
        private void AddItemsToComboBox(ComboBox comboBox, IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                comboBox.Items.Add(item);
            }
        }
        private void ClearComboBoxes()
        {
            Pipes.Items.Clear();
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

        private void SupValFl_Click(object sender, RoutedEventArgs e)
        {
            Window imageWindow = new Window
            {
                Title = "Информация по выбору типа кабеля",
                Width = 500,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/supports.png"));

            imageWindow.Content = image;
            imageWindow.ShowDialog();
        }

        private void EditPipes_Click(object sender, RoutedEventArgs e)
        {
            EditPipes editPipes = new EditPipes();
            editPipes.MainEditPipe += () =>
            {
                Pipes.Items.Clear();
                Pipes.ItemsSource = null;
                AddItemsToComboBox(Pipes, Data.UploadedData.Instance.Pipes.Select(pipe => $"{pipe.Name}"));
                Pipes.Items.Refresh();
            };
            if (editPipes.ShowDialog() == true)
            {

            }
        }


        private void ThermalIsolationEditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditInsulation();
            editWindow.InsulationUpdated += () =>
            {
                ThermalIsolationCombobox.Items.Clear();
                ThermalIsolationCombobox.ItemsSource = null;
                ThermalIsolationCombobox2.Items.Clear();
                ThermalIsolationCombobox2.ItemsSource = null;
                AddItemsToComboBox(ThermalIsolationCombobox, Data.UploadedData.Instance.Insulations.Select(insulation => insulation.Name));
                AddItemsToComboBox(ThermalIsolationCombobox2, Data.UploadedData.Instance.Insulations.Select(insulation => insulation.Name));
                ThermalIsolationCombobox.Items.Refresh();
                ThermalIsolationCombobox2.Items.Refresh();
            };
            if (editWindow.ShowDialog() == true)
            {

            }
        }

    }
}
