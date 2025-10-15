using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OfficeOpenXml.Table.PivotTable;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using UncomClc.Models;
using UncomClc.Models.Insulations;
using UncomClc.Service;
using UncomClc.Views.Line;
using static UncomClc.Service.CalculateService;

namespace UncomClc.ViewModels
{
    public class Main : INotifyPropertyChanged
    {
        public string TempFile;
        public ProcessView ProcessVM { get; } = new ProcessView();
        public EnvironmentView EnvironmentVM { get; } = new EnvironmentView();
        public PowerSupplyParametersView PowerSupplyParametersVM { get; } = new PowerSupplyParametersView();

        public ResultView ResultView { get; } = new ResultView();


        private ObservableCollection<GeneralStructure> _pipeLines = new ObservableCollection<GeneralStructure>();
        private GeneralStructure _selectedPipeLine;
        private string file;
        private string tempFilePath;
        private bool _isUpdatingUI;
        private DateTime _lastSaveTime = DateTime.MinValue;
        private string _lastSavedContent;
        private readonly CalculateService calculationService;
        private TextBlock TextBlock;
        private string _currentFileName;
        private bool _isCalculating = false;
        private bool showMessage = false;
        private Parameters reset;

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

                // Выполняем расчет для текущего элемента перед переключением
                if (_selectedPipeLine != null)
                {
                    showMessage = false;
                    ExecuteCalculate(); // Рассчитываем текущую трубу
                    SaveCurrentParameters(); // Сохраняем параметры и результаты
                }

                _selectedPipeLine = value;
                OnPropertyChanged(nameof(SelectedPipeLine));

                // Загружаем данные нового элемента
                if (_selectedPipeLine == null)
                {
                    ResultView.CalculatedHeatLoss = 0;
                }
                else
                {
                    LoadSelectedPipelineData();
                }
            }
        }


        public ICommand AddPipeCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditLineNameCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand InfoCommand { get; }
        public ICommand Calculate { get; }

        public string CurrentFile
        {
            get => _currentFileName;
            set
            {
                _currentFileName = value;
                OnPropertyChanged(nameof(CurrentFile));
            }
        }

        public Main()
        {
            calculationService = new CalculateService();
            reset = UpdateCurrentParameters();
            // Инициализация команд
            AddPipeCommand = new RelayCommand(AddNewPipe);
            DeleteCommand = new RelayCommand(DeleteSelectedPipe);
            CreateCommand = new RelayCommand(CreateFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile);
            InfoCommand = new RelayCommand(ShowInfo);
            EditLineNameCommand = new RelayCommand(EditLineName);
            CopyCommand = new RelayCommand(CopyRow);
            Calculate = new RelayCommand(CalculateWithMessages);

            ProcessVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
            EnvironmentVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
            PowerSupplyParametersVM.PropertyChanged += (s, e) => OnChildPropertyChanged();
            ResultView.PropertyChanged += (s, e) => OnChildPropertyChanged();
            CurrentFile = "Нет файла";

            PipeLines = new ObservableCollection<GeneralStructure>();
        }
        private void OnChildPropertyChanged()
        {
            if (!_isUpdatingUI && SelectedPipeLine != null)
            {
                // Сбрасываем флаг успешного расчета при изменении параметров
                if (SelectedPipeLine.SuccessCalculation)
                {
                    SelectedPipeLine.SuccessCalculation = false;
                    OnPropertyChanged(nameof(SelectedPipeLine));
                }
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

            var currentParams = UpdateCurrentParameters();
            if (currentParams.Diam < 2 * currentParams.Thickness)
            {
                MessageBox.Show("Диаметр трубы не может быть меньше двух толщин стенки", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Рассчитываем текущую выбранную трубу перед добавлением новой
            if (SelectedPipeLine != null)
            {
                showMessage = false;
                ExecuteCalculate(); // Рассчитываем текущую трубу
                SaveCurrentParameters(); // Сохраняем параметры и результаты
            }
            var newId = PipeLines.Any() ? PipeLines.Max(p => p.Id) + 1 : 1;
            var newStructure = new GeneralStructure
            {
                Id = newId,
                Name = $"NewLine_{newId}",
                HasWarning = true,
                SuccessCalculation = false,
                Parameters = UpdateCurrentParameters(),
                CalculateResult = UpdateCurrentResult()
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
                var copiedresult = JsonSerializer.Deserialize<CalculateResult>(serializedParams, options);

                var newId = PipeLines.Any() ? PipeLines.Max(p => p.Id) + 1 : 1;

                var newStructure = new GeneralStructure
                {
                    Id = newId,
                    Name = $"{SelectedPipeLine.Name}_Copy",
                    Parameters = copiedParams,
                    HasWarning = true,
                    SuccessCalculation = false,
                    CalculateResult = copiedresult
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
            if (!string.IsNullOrEmpty(file)) QuestionBeforeExite();
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Unctrace files (*.unctrace)|*.unctrace|All files (*.*)|*.*\"",
                DefaultExt = ".unctrace",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                PipeLines.Clear();
                file = saveFileDialog.FileName;
                tempFilePath = Path.ChangeExtension(file, ".tmp");
                TempFile = tempFilePath;
                CurrentFile = Path.GetFileName(file);
                // Создаем пустой основной файл
                File.WriteAllText(file, "[]");

                // Создаем временный файл с теми же данными
                File.WriteAllText(tempFilePath, "[]");

                var newStructure = new GeneralStructure
                {
                    Id = 1,
                    Name = "NewLine",
                    Parameters = reset,
                    CalculateResult = new CalculateResult()
                };
                PipeLines.Add(newStructure);
                SelectedPipeLine = newStructure;
                SaveToTempFile();
            }
        }
        private void OpenFile()
        {
            if (!string.IsNullOrEmpty(file)) QuestionBeforeExite();
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Unctrace files (*.unctrace)|*.unctrace|All files (*.*)|*.*\"",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                file = openFileDialog.FileName;
                tempFilePath = Path.ChangeExtension(file, ".tmp");
                TempFile = tempFilePath;
                CurrentFile = Path.GetFileName(file);
                try
                {
                    // 1. Копируем исходный файл во временный
                    File.Copy(file, tempFilePath, overwrite: true);

                    // 2. Загружаем данные из временного файла
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

                // 1. Читаем данные из временного файла
                var json = File.ReadAllText(tempFilePath);

                // 2. Сохраняем данные в основной файл
                File.WriteAllText(file, json);

                // 3. Удаляем временный файл
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                // 4. Создаем новый временный файл для продолжения работы
                tempFilePath = Path.ChangeExtension(file, ".tmp");
                TempFile = tempFilePath;
                File.WriteAllText(tempFilePath, json); // Сохраняем данные в новый временный файл

                _lastSavedContent = json; // Обновляем последнее сохраненное содержимое

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

        private CalculateResult UpdateCurrentResult()
        {
            var result = new CalculateResult();
            result.Rpot = ResultView.CalculatedHeatLoss;
            result.Lobsh = ResultView.Lobsh;
            result.Lzap = ResultView.Lzap;
            result.Lzadv = ResultView.Lzadv;
            result.Lfl = ResultView.Lfl;
            result.Lop = ResultView.Lop;
            result.Pobogr = ResultView.Pobogr;
            result.Pkabrab = ResultView.Pkabrab;
            result.Scheme = ResultView.Scheme;
            result.Ssec = (int)ResultView.Ssec;
            result.Lsec = ResultView.Lsec;
            result.Lust = ResultView.Lust;
            result.TempClass = ResultView.TempClass;
            result.Pit = ResultView.Pit;
            result.Urab = ResultView.Urab;
            result.Psec20 = ResultView.Psec20 * 1000;
            result.Ivklmin = ResultView.Ivklmin;
            result.Irab = ResultView.Irab;
            result.Psecvklmin = ResultView.Psecvklmin;
            result.Psecrab = ResultView.Psecrab;
            result.CH = ResultView.CH;
            result.Mark = ResultView.Mark;
            result.Cross = ResultView.Cross;
            result.Resistance = ResultView.Resistance;
            result.Tobol = ResultView.Tobol;
            result.IsShellTemp = ResultView.IsShellTemp;
            result.IsStartCurrent = ResultView.IsStartCurrent;
            result.IsLenght = ResultView.IsLenght;
            return result;
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
                var result = SelectedPipeLine.CalculateResult;
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
                ProcessVM.TemperatureClass = temp.TemperatureClass;
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

                // result

                ResultView.CalculatedHeatLoss = result.Rpot;
                ResultView.Lobsh = result.Lobsh;
                ResultView.Lzap = result.Lzap;
                ResultView.Lzadv = result.Lzadv;
                ResultView.Lfl = result.Lfl;
                ResultView.Lop = result.Lop;
                ResultView.Pobogr = result.Pobogr;
                ResultView.Pkabrab = result.Pkabrab;
                ResultView.Scheme = SelectedPipeLine.Parameters.ConnectionScheme;
                ResultView.Ssec = result.Ssec;
                ResultView.Lsec = result.Lsec;
                ResultView.Lust = result.Lust;
                ResultView.TempClass = SelectedPipeLine.Parameters.TemperatureClass;
                ResultView.Pit = SelectedPipeLine.Parameters.Nutrition;
                ResultView.Urab = result.Urab;
                ResultView.Psec20 = Math.Round(result.Psec20 / 1000, 2, MidpointRounding.AwayFromZero); ;
                ResultView.Ivklmin = result.Ivklmin;
                ResultView.Irab = result.Irab;
                ResultView.Psecvklmin = result.Psecvklmin;
                ResultView.Psecrab = result.Psecrab;
                ResultView.Lsec = result.Lsec;
                ResultView.CH = result.CH;
                ResultView.Mark = result.Mark;
                ResultView.Cross = result.Cross;
                ResultView.Resistance = result.Resistance;
                ResultView.Tobol = result.Tobol;
                ResultView.IsShellTemp = result.IsShellTemp;
                ResultView.IsStartCurrent = result.IsStartCurrent;
                ResultView.IsLenght = result.IsLenght;

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
            if (string.IsNullOrEmpty(tempFilePath)) return;
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

                // Всегда сохраняем во временный файл
                File.WriteAllText(tempFilePath, currentContent);
                _lastSavedContent = currentContent;
                _lastSaveTime = DateTime.Now;
                Debug.WriteLine($"Автосохранение выполнено: {DateTime.Now}");
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
        public void ExecuteCalculate()
        {
            if (SelectedPipeLine == null) return;
            var result = calculationService.Calculation(SelectedPipeLine, showMessage);
            if (result != null)
            {
                // Сохраняем результат в текущий объект
                SelectedPipeLine.CalculateResult = result;

                // Обновляем отображение
                ResultView.CalculatedHeatLoss = result.Rpot;
                ResultView.HeatCableLength = result.HeatCableLenght;
                ResultView.Lobsh = result.Lobsh;
                ResultView.Lzap = result.Lzap;
                ResultView.Lzadv = result.Lzadv;
                ResultView.Lfl = result.Lfl;
                ResultView.Lop = result.Lop;
                ResultView.Pobogr = result.Pobogr;
                ResultView.Pkabrab = result.Pkabrab;
                ResultView.Scheme = result.Scheme;
                ResultView.Ssec = result.Ssec;
                ResultView.Tobol = result.Tobol;
                ResultView.CH = result.CH;
                ResultView.Mark = result.Mark;
                ResultView.Cross = result.Cross;
                ResultView.Resistance = result.Resistance;
                ResultView.Urab = result.Urab;
                ResultView.Psec20 = result.Psec20;
                ResultView.Lsec = result.Lsec;
                ResultView.Lust = result.Lust;
                ResultView.TempClass = result.TempClass;
                ResultView.Pit = result.Pit;
                ResultView.Ivklmin = result.Ivklmin;
                ResultView.Irab = result.Irab;
                ResultView.Psecvklmin = result.Psecvklmin;
                ResultView.Psecrab = result.Psecrab;
                ResultView.IsShellTemp = result.IsShellTemp;
                ResultView.IsStartCurrent = result.IsStartCurrent;
                ResultView.IsLenght = result.IsLenght;

                SelectedPipeLine.SuccessCalculation = true;
                // Сохраняем изменения
                SaveToTempFile();

                // Уведомляем об изменении (если нужно)
                OnPropertyChanged(nameof(SelectedPipeLine));
            }
        }

        public void CalculateWithMessages()
        {
            showMessage = true;
            ExecuteCalculate();
        }
        private void QuestionBeforeExite()
        {
            var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Подтверждение",
                           MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveFile();
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        private void ShowInfo()
        {
            Window imageWindow = new Window
            {
                Title = "Информация",
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };


            // Создание основного контейнера
            StackPanel mainPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Background = Brushes.White
            };

            // Заголовок
            TextBlock title = new TextBlock
            {
                Text = "Контактная информация",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Изображение
            Image logo = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/uncomtech.png")),
                Width = 150,
                Height = 150,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Контактная информация
            StackPanel contactPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            contactPanel.Children.Add(CreateContactItem("Телефон:", "8 (800) 600-10-20 (по всей стране)"));
            contactPanel.Children.Add(CreateContactItem("", "+7 (499) 277-17-50 (Московская область)"));
            contactPanel.Children.Add(CreateContactItem("Email:", "https://www.uncomtech.ru/"));
            contactPanel.Children.Add(CreateContactItem("Адрес:", "г. Москва, ул. Большая Ордынка, д. 46с5"));


            // Добавление элементов в основную панель
            mainPanel.Children.Add(title);
            mainPanel.Children.Add(logo);
            mainPanel.Children.Add(contactPanel);

            imageWindow.Content = mainPanel;
            imageWindow.ShowDialog();
        }
        private StackPanel CreateContactItem(string label, string value)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0)
            };
            if (label == "Email:")
            {
                Hyperlink hyperlink = new Hyperlink { NavigateUri = new Uri("mailto:info@uncomtech.com") };
                Hyperlink hyperlink2 = new Hyperlink { NavigateUri = new Uri("mailto:mtulyakov@uncomtech.com") };
                hyperlink.Inlines.Add("info@uncomtech.com");
                hyperlink2.Inlines.Add("mtulyakov@uncomtech.com");
                hyperlink.RequestNavigate += (sender, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = e.Uri.ToString(),
                        UseShellExecute = true
                    });
                };
                hyperlink2.RequestNavigate += (sender, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = e.Uri.ToString(),
                        UseShellExecute = true
                    });
                };

                TextBlock valueText = new TextBlock();
                valueText.Inlines.Add(hyperlink);
                valueText.Inlines.Add(Environment.NewLine);
                valueText.Inlines.Add(hyperlink2);

                panel.Children.Add(labelText);
                panel.Children.Add(valueText);
            }
            else
            {
                TextBlock valueText = new TextBlock { Text = value };
                panel.Children.Add(labelText);
                panel.Children.Add(valueText);
            }

            return panel;
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

