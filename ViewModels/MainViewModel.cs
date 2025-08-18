using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using UncomClc.Models;
using UncomClc.Views.Line;

namespace UncomClc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public string TempFile;
        public ProcessView ProcessVM { get; } = new ProcessView();
        public EnvironmentView EnvironmentVM { get; } = new EnvironmentView();
        public PowerSupplyParametersView PowerSupplyParametersVM { get; } = new PowerSupplyParametersView();


        private ObservableCollection<GeneralStructure> _pipeLines = new ObservableCollection<GeneralStructure>();
        private GeneralStructure _selectedPipeLine;
        private string file;
        private string tempFilePath;
        private bool _isUpdatingUI;
        private DateTime _lastSaveTime = DateTime.MinValue;
        private string _lastSavedContent;

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
                if (_selectedPipeLine == value) return;
                _selectedPipeLine = value;
                OnPropertyChanged(nameof(SelectedPipeLine));

            }
        }


        public ICommand AddPipeCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditLineNameCommand { get; }
        public ICommand CopyCommand { get; }

        public MainViewModel()
        {
            // Инициализация команд
            AddPipeCommand = new RelayCommand(AddNewPipe);
            DeleteCommand = new RelayCommand(DeleteSelectedPipe);
            CreateCommand = new RelayCommand(CreateFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile);
            EditLineNameCommand = new RelayCommand(EditLineName);
            CopyCommand = new RelayCommand(CopyRow);

            ProcessVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
            EnvironmentVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
            PowerSupplyParametersVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
        }
        private void OnChildPropertyChanged()
        {
            if (!_isUpdatingUI && SelectedPipeLine != null)
            {
                SaveCurrentParameters();
                SaveToTempFile();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddNewPipe()
        {
            if (string.IsNullOrEmpty(file)) return;
            var newId = PipeLines.Any() ? PipeLines.Max(p => p.Id) + 1 : 1;
            var newStructure = new GeneralStructure
            {
                Id = newId,
                Name = $"NewLine_{newId}",
                Parameters = UpdateCurrentParameters()
            };

            PipeLines.Add(newStructure);
            SelectedPipeLine = newStructure;
            SaveToTempFile();
        }
        private void CopyRow()
        {
            if (string.IsNullOrEmpty(file)) return;
            if (SelectedPipeLine == null) return;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var serializedParams = JsonSerializer.Serialize(SelectedPipeLine.Parameters, options);
                var copiedParams = JsonSerializer.Deserialize<Parameters>(serializedParams, options);

                var newId = PipeLines.Any() ? PipeLines.Max(p => p.Id) + 1 : 1;

                var newStructure = new GeneralStructure
                {
                    Id = newId,
                    Name = $"{SelectedPipeLine.Name}_Copy",
                    Parameters = copiedParams
                };

                PipeLines.Add(newStructure);
                SelectedPipeLine = newStructure;
                SaveToTempFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                SaveToTempFile();
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
                tempFilePath = Path.ChangeExtension(file, ".tmp");
                TempFile = tempFilePath;
                var newStructure = new GeneralStructure
                {
                    Id = 1,
                    Name = "NewLine",
                    Parameters = UpdateCurrentParameters()
                };
                PipeLines.Add(newStructure);
                SelectedPipeLine = newStructure;
                SaveToTempFile();

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
                tempFilePath = Path.ChangeExtension(file, ".tmp");
                TempFile = tempFilePath;
                try
                {
                    // Копируем исходный файл во временный
                    File.Copy(file, tempFilePath, overwrite: true);

                    // Загружаем данные из временного файла
                    LoadFromTempFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void SaveFile()
        {
            if (string.IsNullOrEmpty(file))
            {
                MessageBox.Show("Файл не создан/не открыт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // 1. Сохраняем текущие изменения во временный файл
                var json = JsonSerializer.Serialize(PipeLines, options);
                File.WriteAllText(tempFilePath, json);

                // 2. Копируем временный файл в основной
                File.Copy(tempFilePath, file, overwrite: true);

                // 3. Удаляем временный файл
                File.Delete(tempFilePath);
                tempFilePath = null;
                TempFile = null;
                MessageBox.Show("Файл успешно сохранён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public void SaveCurrentParameters()
        {
            if (_isUpdatingUI || SelectedPipeLine == null) return;
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
        public void LoadSelectedPipelineData()
        {
            if (_isUpdatingUI || SelectedPipeLine == null || SelectedPipeLine.Parameters == null)
                return;

            try
            {
                _isUpdatingUI = true;
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
            finally
            {
                _isUpdatingUI = false;
            }
            SaveToTempFile();
        }
        private void EditLineName()
        {
            if (SelectedPipeLine == null) return;

            var lineView = new LineView(SelectedPipeLine.Name);
            if (lineView.ShowDialog() == true && !string.IsNullOrWhiteSpace(lineView.NameLine))
            {
                SelectedPipeLine.Name = lineView.NameLine.Trim();

                var index = PipeLines.IndexOf(SelectedPipeLine);
                PipeLines.RemoveAt(index);
                PipeLines.Insert(index, SelectedPipeLine);

                OnPropertyChanged(nameof(PipeLines));
            }
        }

        public void SaveToTempFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var currentContent = JsonSerializer.Serialize(PipeLines, options);

                // Оптимизация: сохраняем только если данные изменились
                if (currentContent != _lastSavedContent || (DateTime.Now - _lastSaveTime).TotalSeconds > 5)
                {
                    File.WriteAllText(tempFilePath, currentContent);
                    _lastSavedContent = currentContent;
                    _lastSaveTime = DateTime.Now;
                    Debug.WriteLine($"Автосохранение выполнено: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка автосохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadFromTempFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = File.ReadAllText(tempFilePath);
                var structures = JsonSerializer.Deserialize<ObservableCollection<GeneralStructure>>(json, options);

                // Создаем новую коллекцию вместо очистки старой
                var newPipeLines = new ObservableCollection<GeneralStructure>();

                foreach (var structure in structures)
                {
                    newPipeLines.Add(structure);
                }

                // Полностью заменяем коллекцию (это вызовет обновление UI)
                PipeLines = newPipeLines;

                // Выбираем первый элемент
                SelectedPipeLine = PipeLines.FirstOrDefault();

                // Принудительно обновляем привязки
                OnPropertyChanged(nameof(PipeLines));
                OnPropertyChanged(nameof(SelectedPipeLine));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
    public static class ObservableCollectionExtensions
    {
        public static void Move<T>(this ObservableCollection<T> collection, int oldIndex, int newIndex)
        {
            var item = collection[oldIndex];
            collection.RemoveAt(oldIndex);
            collection.Insert(newIndex, item);
        }
    }
}

