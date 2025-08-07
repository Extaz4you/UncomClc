using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using UncomClc.Data;
using UncomClc.Models;
using UncomClc.Views.PipeViews;

namespace UncomClc.Views
{
    /// <summary>
    /// Логика взаимодействия для EditPipes.xaml
    /// </summary>
    public partial class EditPipes : Window
    {
        public event Action MainEditPipe;
        public EditPipes()
        {
            InitializeComponent();
            LoadPipes();
        }
        private void LoadPipes()
        {
            PipesDataGrid.ItemsSource = Data.UploadedData.Instance.Pipes;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var pipePropertiesWindow = new Add_Pipe();
            if (pipePropertiesWindow.ShowDialog() == true)
            {
                var newPipe = pipePropertiesWindow.NewPipe;
                if (newPipe != null)
                {
                    MainEditPipe?.Invoke();
                    PipesDataGrid.Items.Refresh();
                }

            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = PipesDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var pipeItem = selectedItem as Models.Pipe;

                var editWindow = new Change_Pipe(pipeItem);
                if (editWindow.ShowDialog() == true)
                {
                    PipesDataGrid.ItemsSource = null;
                    PipesDataGrid.ItemsSource = UploadedData.Instance.Pipes;
                    PipesDataGrid.Items.Refresh();
                    MainEditPipe?.Invoke();
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
            var selectedItem = PipesDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var pipeToRemove = selectedItem as Models.Pipe;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить трубу '{pipeToRemove.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        UploadedData.Instance.Pipes.Remove(pipeToRemove);

                        UploadedData.Instance.Save();

                        PipesDataGrid.ItemsSource = null;
                        PipesDataGrid.ItemsSource = UploadedData.Instance.Pipes;

                        MainEditPipe?.Invoke();
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
