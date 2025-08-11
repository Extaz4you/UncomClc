using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UncomClc.ViewModels
{
    public class PowerSupplyParametersView : INotifyPropertyChanged
    {
        private List<string> _allConnectionSchemeOptions = new List<string>() { "линия", "петля", "звезда", "две петли", "две звезды", "три петли", "три звезды" };
        private const double Sqrt3 = 1.73205080757;
        private double phaseVoltage = 220;
        private double lineVoltage = (220 * Sqrt3);
        private int current = 25;
        private int numberCores = 1;
        private bool isRecalculating = false;
        private int lenghtSection = 2;
        private string cableType = "КНМС825";
        private int minTempOn = -20;
        private string connectionScheme = "петля";
        private string nutrition = "однофазное";
        private int workLoad = 0;
        private bool _isUpdatingWorkEnvironment = false;
        private string _workEnvironment = "-";
        public List<string> _allCableTypes = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
        private List<string> _cableTypeOptions;
        private int supportedTemp = 5;
        private int maxTechProductTemp = 20;
        private List<string> filteredCables = new List<string>();

        public int SupportedTemp
        {
            get => supportedTemp;
            set
            {
                if (!CheckAllParam(WorkEnvironment, CableType, value, MaxTechProductTemp))
                {
                    return;
                }
                supportedTemp = value;
                OnPropertyChanged(nameof(SupportedTemp));
                UpdateCableTypeOptions();
                CheckTemp(value, SupportedTemp);
            }
        }
        public int MaxTechProductTemp
        {
            get => maxTechProductTemp;
            set
            {
                if (!CheckAllParam(WorkEnvironment, CableType, SupportedTemp, value))
                {
                    return;
                }
                maxTechProductTemp = value;
                OnPropertyChanged(nameof(MaxTechProductTemp));
                UpdateCableTypeOptions();
                CheckTemp(SupportedTemp, value);
            }
        }

        public int PhaseVoltage
        {
            get => Convert.ToInt32(phaseVoltage);
            set
            {
                if (numberCores == 1 && (value < 0 || value > 660))
                {
                    MessageBox.Show("Фазное напряжение должно быть в диапазоне от 0 до 660", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (numberCores == 2 && (value < 0 || value > 600))
                {
                    MessageBox.Show("Фазное напряжение должно быть в диапазоне от 0 до 600", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                phaseVoltage = value;
                OnPropertyChanged(nameof(PhaseVoltage));
                OnPropertyChanged(nameof(WorkLoad));
                UpdateNumberCoresOptions();
                if (!isRecalculating)
                {
                    isRecalculating = true;
                    LineVoltage = Convert.ToInt32(value * Sqrt3);
                    isRecalculating = false;
                }
            }
        }

        public int LineVoltage
        {
            get => Convert.ToInt32(lineVoltage);
            set
            {
                if (numberCores == 1 && (value < 0 || value > 1143))
                {
                    MessageBox.Show("Линейное напряжение должно быть в диапазоне от 0 до 1143", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (numberCores == 2 && (value < 0 || value > 1039))
                {
                    MessageBox.Show("Линейное напряжение должно быть в диапазоне от 0 до 1039", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                lineVoltage = value;
                OnPropertyChanged(nameof(LineVoltage));
                OnPropertyChanged(nameof(WorkLoad));
                if (!isRecalculating)
                {
                    isRecalculating = true;
                    PhaseVoltage = Convert.ToInt32(value / Sqrt3);
                    isRecalculating = false;
                }
            }
        }

        public int Current
        {
            get => current;
            set
            {
                current = value;
                OnPropertyChanged(nameof(Current));

            }
        }

        public int NumberCores
        {
            get => numberCores;
            set
            {
                // Проверяем, изменилось ли значение
                if (numberCores == value) return;

                // Проверка ограничений
                if ((phaseVoltage > 600 || connectionScheme == "линия") && value == 2)
                {
                    MessageBox.Show(phaseVoltage > 600
                        ? "При фазном напряжении свыше 600В нельзя выбрать 2 жилы"
                        : "Для схемы 'линия' можно выбрать только 1 жилу",
                        "Ошибка выбора", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                numberCores = value;
                OnPropertyChanged(nameof(NumberCores));
                PhaseVoltage = Convert.ToInt32(phaseVoltage);
                LineVoltage = Convert.ToInt32(lineVoltage);
                UpdateNumberCoresOptions();
            }
        }

        public int LenghtSection
        {
            get => lenghtSection;
            set
            {
                lenghtSection = value;
                OnPropertyChanged(nameof(LenghtSection));
            }
        }

        public string CableType
        {
            get => cableType;
            set
            {
                cableType = value;
                OnPropertyChanged(nameof(CableType));
            }
        }

        public int MinTempOn
        {
            get => minTempOn;
            set
            {
                minTempOn = value;
                OnPropertyChanged(nameof(MinTempOn));
            }
        }

        public string ConnectionScheme
        {
            get => connectionScheme;
            set
            {
                connectionScheme = value;
                OnPropertyChanged(nameof(ConnectionScheme));
                UpdateNumberCoresOptions();
            }
        }

        public string Nutrition
        {
            get => nutrition;
            set
            {
                nutrition = value;
                OnPropertyChanged(nameof(Nutrition));
                OnPropertyChanged(nameof(WorkLoad));
                UpdateConnectionSchemeOptions();
            }
        }

        public int WorkLoad
        {
            get => Nutrition == "однофазное" ? Convert.ToInt32(phaseVoltage) : Convert.ToInt32(lineVoltage);
            set
            {
                workLoad = value;
                OnPropertyChanged(nameof(WorkLoad));
            }
        }
        public string WorkEnvironment
        {
            get => _workEnvironment;
            set
            {
                if (_isUpdatingWorkEnvironment || _workEnvironment == value)
                    return;

                _isUpdatingWorkEnvironment = true;

                try
                {
                    if (!CheckAllParam(value, CableType, SupportedTemp, MaxTechProductTemp))
                    {
                        // Возвращаем предыдущее значение в UI
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            OnPropertyChanged(nameof(WorkEnvironment));
                        }), System.Windows.Threading.DispatcherPriority.Background);
                        return;
                    }

                    _workEnvironment = value;
                    OnPropertyChanged(nameof(WorkEnvironment));
                    UpdateCableTypeOptions();
                }
                finally
                {
                    _isUpdatingWorkEnvironment = false;
                }
            }
        }
        public List<string> CableTypeOptions
        {
            get => _cableTypeOptions ?? _allCableTypes;
            private set
            {
                _cableTypeOptions = value;
                OnPropertyChanged(nameof(CableTypeOptions));
                UpdateCableSelection();
            }
        }



        public List<string> WorkEnvironmentOptions { get; } = new List<string> { "серная кислота", "соляная кислота", "плавиковая кислота", "фосфорная кислота", "азотная кислота", "органические кислоты", "щелочи", "соли", "морская вода", "хлориды", "-" };
        //public List<string> CableTypeOptions { get; } = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
        public List<string> NutritionOptions { get; private set; } = new List<string>() { "однофазное", "двухфазное", "трехфазное" };
        public List<string> ConnectionSchemeOptions { get; private set; } = new List<string>() { "линия", "петля", "две петли", "три петли", };
        public List<int> NumberCoresOptions { get; private set; } = new List<int>() { 1, 2 };

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
        }


        private void UpdateConnectionSchemeOptions()
        {
            if (Nutrition == "трехфазное")
            {

                ConnectionSchemeOptions = new List<string>(_allConnectionSchemeOptions.Where(x => x.Contains("звезда") || x.Contains("звезды")));
            }
            else
            {

                ConnectionSchemeOptions = new List<string>(_allConnectionSchemeOptions.Where(x => !x.Contains("звезда") && !x.Contains("звезды")));
            }

            OnPropertyChanged(nameof(ConnectionSchemeOptions));


            if (!ConnectionSchemeOptions.Contains(connectionScheme))
            {
                ConnectionScheme = ConnectionSchemeOptions.FirstOrDefault();
            }
        }
        private void UpdateNumberCoresOptions()
        {
            var newOptions = new List<int>();

            if (phaseVoltage > 600 || connectionScheme == "линия")
            {
                newOptions.Add(2);
            }
            else
            {
                newOptions.AddRange(new[] { 1, 2 });
            }

            if (!NumberCoresOptions.SequenceEqual(newOptions))
            {
                NumberCoresOptions = newOptions;
                OnPropertyChanged(nameof(NumberCoresOptions));

                if (!NumberCoresOptions.Contains(numberCores))
                {
                    numberCores = NumberCoresOptions.FirstOrDefault();
                    OnPropertyChanged(nameof(NumberCores));
                }
            }
        }
        private void UpdateCableTypeOptions()
        {
            filteredCables = new List<string>();
            switch (WorkEnvironment)
            {
                case "серная кислота":
                    filteredCables = _allCableTypes
                        .Where(t => t == "КНМСин" || t == "КНМС825")
                        .ToList();
                    break;

                case "соляная кислота":
                    filteredCables = _allCableTypes
                        .Where(t => t == "КНММН" || t == "КНМСин" || t == "КНМС825")
                        .ToList();
                    break;

                case "плавиковая кислота":
                case "фосфорная кислота":
                    filteredCables = _allCableTypes
                        .Where(t => t != "КНМС")
                        .ToList();
                    break;

                case "азотная кислота":
                    filteredCables = _allCableTypes
                        .Where(t => t != "КНММ")
                        .ToList();
                    break;

                case "органические кислоты":

                case "щелочи":
                case "соли":
                    filteredCables = new List<string>(_allCableTypes);
                    break;

                case "морская вода":
                    filteredCables = _allCableTypes
                        .Where(t => t != "КНММ" && t != "КНМС")
                        .ToList();
                    break;

                case "хлориды":
                    filteredCables = _allCableTypes
                        .Where(t => t != "КНМС")
                        .ToList();
                    break;

                default:
                    filteredCables = new List<string>(_allCableTypes);
                    break;
            }

            CableTypeOptions = filteredCables;
        }
        private void UpdateCableSelection()
        {
            if (!CableTypeOptions.Contains(CableType))
            {
                CableType = CableTypeOptions.FirstOrDefault();
            }
        }
        private bool CheckAllParam(string environment, string cable, int supportedTemp, int maxTechTemp)
        {
            if (environment == "серная кислота" && cable != "КНМСин" && cable != "КНМС825")
            {
                MessageBox.Show("Серная кислота допустима только для кабелей КНМСин или КНМС825",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "соляная кислота" && cable != "КНМСин" && cable != "КНМС825" && cable != "КНММН")
            {
                MessageBox.Show("Соляная кислота допустима только для кабелей КНМСин, КНМС825 или КНММН",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "плавиковая кислота" && cable == "КНМС")
            {
                MessageBox.Show("Плавиковая кислота недопустима для кабеля КНМС",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "фосфорная кислота" && cable == "КНМС")
            {
                MessageBox.Show("Фосфорная кислота недопустима для кабеля КНМС",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "азотная кислота" && cable == "КНММ")
            {
                MessageBox.Show("Азотная кислота недопустима для кабеля КНММ",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "морская вода" && (cable == "КНММ" || cable == "КНМС"))
            {
                MessageBox.Show("Морская вода недопустима для кабелей КНММ и КНМС",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (environment == "хлориды" && cable == "КНМС")
            {
                MessageBox.Show("Хлориды недопустимы для кабеля КНМС",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((supportedTemp > 200 || maxTechTemp > 250) && cable == "КНММ")
            {
                MessageBox.Show("Данный кабель недопустим при указанных температурных параметрах",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((supportedTemp > 600 || maxTechTemp > 800) && cable == "КНММН")
            {
                MessageBox.Show("Данный кабель недопустим при указанных температурных параметрах",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((supportedTemp > 600 || maxTechTemp > 800) && cable == "КНМС")
            {
                MessageBox.Show("Данный кабель недопустим при указанных температурных параметрах",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((supportedTemp > 600 || maxTechTemp > 1000) && cable == "КНМСин")
            {
                MessageBox.Show("Данный кабель недопустим при указанных температурных параметрах",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((supportedTemp > 650 || maxTechTemp > 800) && cable == "КНМС825")
            {
                MessageBox.Show("Данный кабель недопустим при указанных температурных параметрах",
                              "Ошибка выбора кабеля", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CheckTemp(int sup, int max)
        {
            UpdateCableTypeOptions();
            var tempFilteredCables = new List<string>(filteredCables);
            if (sup > 200 || max > 250)
            {
                tempFilteredCables.Remove("КНММ");
            }

            if (sup > 400 || max > 600)
            {
                tempFilteredCables.Remove("КНММН");
            }

            if (sup > 600 || max > 800)
            {
                tempFilteredCables.Remove("КНМС");
            }

            if (sup > 600 || max > 1000)
            {
                tempFilteredCables.Remove("КНМСин");
            }

            if (sup > 650 || max > 800)
            {
                tempFilteredCables.Remove("КНМС825");
            }
            CableTypeOptions = tempFilteredCables;
        }
    }
}
