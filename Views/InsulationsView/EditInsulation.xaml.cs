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
using System.Windows.Shapes;
using UncomClc.Data;
using UncomClc.Models.Insulations;
using UncomClc.Views.PipeViews;

namespace UncomClc.Views.InsulationsView
{
    /// <summary>
    /// Логика взаимодействия для EditInsulation.xaml
    /// </summary>
    public partial class EditInsulation : Window
    {
        public event Action InsulationUpdated;
        public EditInsulation()
        {
            InitializeComponent();
            Load();
        }
        private void Load()
        {
            InsulationDataGrid.ItemsSource = Data.UploadedData.Instance.Insulations;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var pipePropertiesWindow = new Add_Insulation();
            if (pipePropertiesWindow.ShowDialog() == true)
            {
                var newInsulation = pipePropertiesWindow.NewInsulation;
                if (newInsulation != null)
                {
                    InsulationUpdated?.Invoke();
                    InsulationDataGrid.Items.Refresh();
                }

            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = InsulationDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var insulationItem = selectedItem as Insulation;

                var editWindow = new Change_Insulation(insulationItem);
                if (editWindow.ShowDialog() == true)
                {
                    InsulationDataGrid.ItemsSource = null;
                    InsulationDataGrid.ItemsSource = UploadedData.Instance.Insulations;
                    InsulationDataGrid.Items.Refresh();
                    InsulationUpdated?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для редактирования.");
                return;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = InsulationDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var insulationToRemove = selectedItem as Insulation;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить трубу '{insulationToRemove.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        UploadedData.Instance.Insulations.Remove(insulationToRemove);

                        UploadedData.Instance.Save();

                        InsulationDataGrid.ItemsSource = null;
                        InsulationDataGrid.ItemsSource = UploadedData.Instance.Insulations;

                        InsulationUpdated?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
