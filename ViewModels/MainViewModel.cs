using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using UncomClc.Models;

namespace UncomClc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ProcessView ProcessVM { get; } = new ProcessView();
        public EnvironmentView EnvironmentVM { get; } = new EnvironmentView();
        public PowerSupplyParametersView PowerSupplyParametersVM { get; } = new PowerSupplyParametersView();


        private ObservableCollection<GeneralStructure> _pipeLines = new ObservableCollection<GeneralStructure>();
        private GeneralStructure _selectedPipeLine;

        public ObservableCollection<GeneralStructure> PipeLines
        {
            get => _pipeLines;
            set
            {
                _pipeLines = value;
                OnPropertyChanged(nameof(PipeLines));
            }
        }

        public GeneralStructure SelectedPipeLine
        {
            get => _selectedPipeLine;
            set
            {
                _selectedPipeLine = value;
                OnPropertyChanged(nameof(SelectedPipeLine));
            }
        }


        public ICommand AddPipeCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel()
        {
            // Инициализация команд
            AddPipeCommand = new RelayCommand(AddNewPipe);
            DeleteCommand = new RelayCommand(DeleteSelectedPipe);
            CreateCommand = new RelayCommand(CreateFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void AddNewPipe()
        {
            var newId = PipeLines.Any() ? PipeLines.Max(p => p.Id) + 1 : 1;
            var newStructure = new GeneralStructure
            {
                Id = newId,
                Name = $"NewLine_{newId}",
                Parameters = new Parameters()
            };

            PipeLines.Add(newStructure);
            SelectedPipeLine = newStructure;
        }
        private void DeleteSelectedPipe()
        {
            if (SelectedPipeLine == null)
            {
                MessageBox.Show("Выберите линию для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Удалить линию '{SelectedPipeLine.Name}'?", "Подтверждение",
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PipeLines.Remove(SelectedPipeLine);
                SelectedPipeLine = PipeLines.FirstOrDefault();
            }
        }
        private void CreateFile()
        {
            var newStructure = new GeneralStructure
            {
                Id = 1,
                Name = "NewLine",
                Parameters = new Parameters()
            };
            PipeLines.Add(newStructure);
            SelectedPipeLine = newStructure;
            SaveFile();
        }
        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var structures = JsonSerializer.Deserialize<ObservableCollection<GeneralStructure>>(json);

                    PipeLines.Clear();
                    foreach (var structure in structures)
                    {
                        PipeLines.Add(structure);
                    }

                    if (PipeLines.Count > 0)
                    {
                        SelectedPipeLine = PipeLines[0];
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void SaveFile()
        {
            if (PipeLines.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var json = JsonSerializer.Serialize(PipeLines);
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("Файл успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
