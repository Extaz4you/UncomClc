using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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
        private string file;

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
                LoadSelectedPipelineData();
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
                Parameters = UpdateCurrentParameters()
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
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                PipeLines.Clear();
                file = saveFileDialog.FileName;
                var newStructure = new GeneralStructure
                {
                    Id = 1,
                    Name = "NewLine",
                    Parameters = UpdateCurrentParameters()
                };
                PipeLines.Add(newStructure);
                SelectedPipeLine = newStructure;
                SaveFile();

            }

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
                file = openFileDialog.FileName;
                try
                {
                    var json = File.ReadAllText(file);
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

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                SaveCurrentParameters();
                var json = JsonSerializer.Serialize(PipeLines, options);
                File.WriteAllText(file, json);
                MessageBox.Show("Файл успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Parameters UpdateCurrentParameters()
        {
            
            var param = new Parameters();
            var pipe = Data.UploadedData.Instance.Pipes.Where(x => x.Name == ProcessVM.Pipe).FirstOrDefault();
            var isulation = Data.UploadedData.Instance.Insulations.Where(x => x.Name == ProcessVM.ThermalIsolation).FirstOrDefault();
            var isulation2 = Data.UploadedData.Instance.Insulations.Where(x => x.Name == ProcessVM.ThermalIsolation2).FirstOrDefault();
            
            param.Pipe = pipe;
            param.ThermalIsolation = isulation;
            param.ThermalIsolation2 = isulation2;
            param.IsolationThickness = ProcessVM.IsolationThickness;
            param.IsolationThickness2 = ProcessVM.IsolationThickness2;
            param.Diam = ProcessVM.Diam;
            param.Thickness = ProcessVM.Thickness;
            param.Lenght = ProcessVM.Lenght;
            param.PipeKoef = ProcessVM.PipeKoef;
            param.ValveCount = ProcessVM.ValveCount;
            param.ValveLenght = ProcessVM.ValveLenght;
            param.SupportLenght = ProcessVM.SupportLenght;
            param.SupportCount = ProcessVM.SupportCount;
            param.FlangCount = ProcessVM.FlangCount;
            param.FlangLength = ProcessVM.FlangLength;
            param.MinEnvironmentTemp = EnvironmentVM.MinEnvironmentTemp;
            param.MaxEnvironmentTemp = EnvironmentVM.MaxEnvironmentTemp;
            param.PipelinePlacement = EnvironmentVM.PipelinePlacement;
            param.SupportedTemp = PowerSupplyParametersVM.SupportedTemp;
            param.MaxAddProductTemp = ProcessVM.MaxAddProductTemp;
            param.MaxTechProductTemp = PowerSupplyParametersVM.MaxTechProductTemp;
            param.SteamingStatus = ProcessVM.SteamingStatus;
            param.StreamingTemperature = ProcessVM.StreamingTemperature;
            param.TemperatureClass = ProcessVM.TemperatureClass;
            param.TemperatureClassValue = ProcessVM.TemperatureClassValue;
            param.WorkEnvironment = PowerSupplyParametersVM.WorkEnvironment;
            param.PhaseVoltage = PowerSupplyParametersVM.PhaseVoltage;
            param.LineVoltage = PowerSupplyParametersVM.LineVoltage;
            param.Current = PowerSupplyParametersVM.Current;
            param.Nutrition = PowerSupplyParametersVM.Nutrition;
            param.WorkLoad = PowerSupplyParametersVM.WorkLoad;
            param.ConnectionScheme = PowerSupplyParametersVM.ConnectionScheme;
            param.MinTempOn = PowerSupplyParametersVM.MinTempOn;
            param.CableType = PowerSupplyParametersVM.CableType;
            param.NumberCores = PowerSupplyParametersVM.NumberCores;
            param.LenghtSection = PowerSupplyParametersVM.LenghtSection;
            return param;
        }
        private void SaveCurrentParameters()
        {
            if (SelectedPipeLine == null) return;
            var param = SelectedPipeLine.Parameters;
            var pipe = Data.UploadedData.Instance.Pipes.Where(x => x.Name == ProcessVM.Pipe).FirstOrDefault();
            var isulation = Data.UploadedData.Instance.Insulations.Where(x => x.Name == ProcessVM.ThermalIsolation).FirstOrDefault();
            var isulation2 = Data.UploadedData.Instance.Insulations.Where(x => x.Name == ProcessVM.ThermalIsolation2).FirstOrDefault();

            param.Pipe = pipe;
            param.ThermalIsolation = isulation;
            param.ThermalIsolation2 = isulation2;
            param.IsolationThickness = ProcessVM.IsolationThickness;
            param.IsolationThickness2 = ProcessVM.IsolationThickness2;
            param.Diam = ProcessVM.Diam;
            param.Thickness = ProcessVM.Thickness;
            param.Lenght = ProcessVM.Lenght;
            param.PipeKoef = ProcessVM.PipeKoef;
            param.ValveCount = ProcessVM.ValveCount;
            param.ValveLenght = ProcessVM.ValveLenght;
            param.SupportLenght = ProcessVM.SupportLenght;
            param.SupportCount = ProcessVM.SupportCount;
            param.FlangCount = ProcessVM.FlangCount;
            param.FlangLength = ProcessVM.FlangLength;
            param.MinEnvironmentTemp = EnvironmentVM.MinEnvironmentTemp;
            param.MaxEnvironmentTemp = EnvironmentVM.MaxEnvironmentTemp;
            param.PipelinePlacement = EnvironmentVM.PipelinePlacement;
            param.SupportedTemp = PowerSupplyParametersVM.SupportedTemp;
            param.MaxAddProductTemp = ProcessVM.MaxAddProductTemp;
            param.MaxTechProductTemp = PowerSupplyParametersVM.MaxTechProductTemp;
            param.SteamingStatus = ProcessVM.SteamingStatus;
            param.StreamingTemperature = ProcessVM.StreamingTemperature;
            param.TemperatureClass = ProcessVM.TemperatureClass;
            param.TemperatureClassValue = ProcessVM.TemperatureClassValue;
            param.WorkEnvironment = PowerSupplyParametersVM.WorkEnvironment;
            param.PhaseVoltage = PowerSupplyParametersVM.PhaseVoltage;
            param.LineVoltage = PowerSupplyParametersVM.LineVoltage;
            param.Current = PowerSupplyParametersVM.Current;
            param.Nutrition = PowerSupplyParametersVM.Nutrition;
            param.WorkLoad = PowerSupplyParametersVM.WorkLoad;
            param.ConnectionScheme = PowerSupplyParametersVM.ConnectionScheme;
            param.MinTempOn = PowerSupplyParametersVM.MinTempOn;
            param.CableType = PowerSupplyParametersVM.CableType;
            param.NumberCores = PowerSupplyParametersVM.NumberCores;
            param.LenghtSection = PowerSupplyParametersVM.LenghtSection;
        }
        private void LoadSelectedPipelineData()
        {
            if (SelectedPipeLine == null || SelectedPipeLine.Parameters == null) return;

            // Блокируем обновление UI во время загрузки
            var temp = SelectedPipeLine.Parameters;

            // ProcessView
            ProcessVM.Pipe = temp.Pipe?.Name;
            ProcessVM.Diam = temp.Diam;
            ProcessVM.Thickness = temp.Thickness;
            ProcessVM.PipeKoef = temp.PipeKoef;
            ProcessVM.Lenght = temp.Lenght;
            ProcessVM.ThermalIsolation = temp.ThermalIsolation?.Name;
            ProcessVM.ThermalIsolation2 = temp.ThermalIsolation2?.Name;
            ProcessVM.IsolationThickness = temp.IsolationThickness;
            ProcessVM.IsolationThickness2 = temp.IsolationThickness2;
            ProcessVM.MaxAddProductTemp = temp.MaxAddProductTemp;
            ProcessVM.SteamingStatus = temp.SteamingStatus;
            ProcessVM.StreamingTemperature = temp.StreamingTemperature;
            ProcessVM.TemperatureClassValue = temp.TemperatureClassValue;
            ProcessVM.SupportCount = temp.SupportCount;
            ProcessVM.SupportLenght = temp.SupportLenght;
            ProcessVM.ValveCount = temp.ValveCount;
            ProcessVM.ValveLenght = temp.ValveLenght;
            ProcessVM.FlangCount = temp.FlangCount;
            ProcessVM.FlangLength = temp.FlangLength;

            // EnvironmentView
            EnvironmentVM.MaxEnvironmentTemp = temp.MaxEnvironmentTemp;
            EnvironmentVM.MinEnvironmentTemp = temp.MinEnvironmentTemp;
            EnvironmentVM.PipelinePlacement = temp.PipelinePlacement;

            // PowerSupplyParametersView
            PowerSupplyParametersVM.SupportedTemp = temp.SupportedTemp;
            PowerSupplyParametersVM.MaxTechProductTemp = temp.MaxTechProductTemp;
            PowerSupplyParametersVM.WorkEnvironment = temp.WorkEnvironment;
            PowerSupplyParametersVM.LineVoltage = temp.LineVoltage;
            PowerSupplyParametersVM.Current = temp.Current;
            PowerSupplyParametersVM.NumberCores = temp.NumberCores;
            PowerSupplyParametersVM.LenghtSection = temp.LenghtSection;
            PowerSupplyParametersVM.CableType = temp.CableType;
            PowerSupplyParametersVM.MinTempOn = temp.MinTempOn;
            PowerSupplyParametersVM.ConnectionScheme = temp.ConnectionScheme;
            PowerSupplyParametersVM.Nutrition = temp.Nutrition;
        }

    }
}

