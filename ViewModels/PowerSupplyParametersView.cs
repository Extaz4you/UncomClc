using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string workEnvironment = "-";
        public List<string> _allCableTypes = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
        private List<string> _cableTypeOptions;
        private List<string> _availableWorkEnvironments;
        private List<string> _allWorkEnvironmentOptions = new List<string> { "серная кислота", "соляная кислота", "плавиковая кислота", "фосфорная кислота", "азотная кислота", "органические кислоты", "щелочи", "соли", "морская вода", "хлориды", "-" };

        public int PhaseVoltage
        {
            get => Convert.ToInt32(phaseVoltage);
            set
            {
                if(numberCores == 1 && (value < 0 || value > 660))
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
                UpdateWorkEnvironmentOptions();
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
            get => workEnvironment;
            set
            {
                workEnvironment = value;
                OnPropertyChanged(nameof(WorkEnvironment));
                UpdateCableTypeOptions();
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
        public List<string> AvailableWorkEnvironments
        {
            get => _availableWorkEnvironments ?? _allWorkEnvironmentOptions;
            private set
            {
                _availableWorkEnvironments = value;
                OnPropertyChanged(nameof(AvailableWorkEnvironments));
            }
        }


        public List<string> WorkEnvironmentOptions => _allWorkEnvironmentOptions;
        //public List<string> CableTypeOptions { get; } = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
        public List<string> NutritionOptions { get; private set; } = new List<string>() { "однофазное", "двухфазное", "трехфазное" };
        public List<string> ConnectionSchemeOptions { get; private set; } = new List<string>() { "линия", "петля",  "две петли",  "три петли",  };
        public List<int> NumberCoresOptions { get; private set; } = new List<int>() { 1, 2};

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
            switch (WorkEnvironment)
            {
                case "серная кислота":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t == "КНМСин" || t == "КНМС825")
                        .ToList();
                    break;

                case "соляная кислота":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t == "КНММН" || t == "КНМСин" || t == "КНМС825")
                        .ToList();
                    break;

                case "плавиковая кислота":
                case "фосфорная кислота":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t != "КНМС")
                        .ToList();
                    break;

                case "азотная кислота":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t != "КНММ")
                        .ToList();
                    break;

                case "органические кислоты":

                case "щелочи":
                case "соли":
                    CableTypeOptions = new List<string>(_allCableTypes);
                    break;

                case "морская вода":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t != "КНММ" && t != "КНМС")
                        .ToList();
                    break;

                case "хлориды":
                    CableTypeOptions = _allCableTypes
                        .Where(t => t != "КНМС")
                        .ToList();
                    break;

                default:
                    CableTypeOptions = new List<string>(_allCableTypes);
                    break;
            }
        }

        private void UpdateWorkEnvironmentOptions()
        {
            var newOptions = new List<string>();

            switch (CableType)
            {
                case "КНММ":
                    newOptions = WorkEnvironmentOptions
                        .Where(e => e != "серная кислота" &&
                                   e != "соляная кислота" &&
                                   e != "плавиковая кислота" &&
                                   e != "фосфорная кислота" &&
                                   e != "азотная кислота" &&
                                   e != "морская вода" &&
                                   e != "хлориды")
                        .ToList();
                    break;

                case "КНММН":
                    newOptions = WorkEnvironmentOptions
                        .Where(e => e != "серная кислота" &&
                                   e != "плавиковая кислота" &&
                                   e != "фосфорная кислота" &&
                                   e != "азотная кислота")
                        .ToList();
                    break;

                case "КНМС":
                    newOptions = WorkEnvironmentOptions
                        .Where(e => e != "серная кислота" &&
                                   e != "соляная кислота" &&
                                   e != "плавиковая кислота" &&
                                   e != "фосфорная кислота" &&
                                   e != "хлориды" &&
                                   e != "морская вода")
                        .ToList();
                    break;

                case "КНМСин":
                case "КНМС825":
                    newOptions = new List<string>(WorkEnvironmentOptions);
                    break;

                default:
                    newOptions = new List<string>(WorkEnvironmentOptions);
                    break;
            }
            AvailableWorkEnvironments = newOptions;

            if (!newOptions.Contains(workEnvironment))
            {
                WorkEnvironment = newOptions.FirstOrDefault() ?? "-";
            }
        }

        private void UpdateCableSelection()
        {
            if (!CableTypeOptions.Contains(CableType))
            {
                CableType = CableTypeOptions.FirstOrDefault();
            }
        }
    }
}
