using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using UncomClc.Models;
using UncomClc.Service;
using UncomClc.ViewModels;
using UncomClc.Views;
using UncomClc.Views.InsulationsView;
using UncomClc.Views.Line;

namespace UncomClc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeneralStructure _draggedItem;
        private Point _startPoint;
        private readonly CalculateService calculateService;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(MessagesBlock);
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
        private void AddPipe_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }
        private void PipeLinesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                // Обновляем SelectedPipeLine во ViewModel
                 viewModel.SelectedPipeLine = e.NewValue as GeneralStructure;
                // Принудительно вызываем загрузку данных
                viewModel.LoadSelectedPipelineData();
            }
        }
        private void PipeLinesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что клик был по элементу TreeView
            if (PipeLinesTree.SelectedItem is GeneralStructure selectedItem &&
                DataContext is MainViewModel vm)
            {
                // Создаем окно редактирования с текущим именем
                var lineView = new LineView(selectedItem.Name);

                if (lineView.ShowDialog() == true && !string.IsNullOrWhiteSpace(lineView.NameLine))
                {
                    // Обновляем имя в модели
                    selectedItem.Name = lineView.NameLine;

                    // Обновляем отображение TreeView
                    var tempList = new ObservableCollection<GeneralStructure>(vm.PipeLines);
                    vm.PipeLines = tempList;

                    // Уведомляем об изменениях
                    vm.OnPropertyChanged(nameof(vm.PipeLines));
                    vm.OnPropertyChanged(nameof(vm.SelectedPipeLine));
                }

                e.Handled = true; // Предотвращаем дальнейшую обработку события
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && !string.IsNullOrEmpty(vm.TempFile))
            {
                var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Подтверждение",
                                           MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    vm.SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                // Удаляем временный файл, если он есть
                try
                {
                    if (File.Exists(vm.TempFile))
                        File.Delete(vm.TempFile);
                }
                catch { /* Игнорируем ошибки удаления */ }
            }

            base.OnClosing(e);
        }
        private void PipeLinesTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _draggedItem = (e.OriginalSource as FrameworkElement)?.DataContext as GeneralStructure;
        }
        private void PipeLinesTree_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
                return;

            var currentPosition = e.GetPosition(null);
            var diff = _startPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var treeView = sender as TreeView;
                if (treeView != null)
                {
                    DragDrop.DoDragDrop(treeView, _draggedItem, DragDropEffects.Move);
                }
            }
        }
        private void PipeLinesTree_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GeneralStructure)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void PipeLinesTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GeneralStructure)))
            {
                var targetItem = (e.OriginalSource as FrameworkElement)?.DataContext as GeneralStructure;
                var draggedItem = e.Data.GetData(typeof(GeneralStructure)) as GeneralStructure;

                if (draggedItem == null || targetItem == null || draggedItem == targetItem)
                    return;

                if (DataContext is MainViewModel vm)
                {
                    // Получаем индексы элементов
                    int oldIndex = vm.PipeLines.IndexOf(draggedItem);
                    int newIndex = vm.PipeLines.IndexOf(targetItem);

                    if (oldIndex != -1 && newIndex != -1)
                    {
                        // Перемещаем элемент в коллекции
                        vm.PipeLines.Move(oldIndex, newIndex);

                        // Обновляем привязки
                        vm.OnPropertyChanged(nameof(vm.PipeLines));
                        vm.OnPropertyChanged(nameof(vm.SelectedPipeLine));

                        // Сохраняем изменения
                        vm.SaveToTempFile();
                    }
                }
            }
            _draggedItem = null;
            e.Handled = true;
        }

    }
}
