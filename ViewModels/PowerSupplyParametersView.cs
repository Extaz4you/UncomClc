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
    public class PowerSupplyParametersView : BaseViewModel
    {
        private List<string> _allConnectionSchemeOptions = new List<string>() { "петля", "звезда", "две петли", "две звезды", "три петли", "три звезды", "линия" };
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


        public int SupportedTemp
        {
            get => supportedTemp;
            set
            {
                if (value < -60 || value > 650)
                {
                    MessageBox.Show("Поддерживаемая температура должна быть в диапазоне от -60 до 650", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                supportedTemp = value;
                OnPropertyChanged(nameof(SupportedTemp));
                UpdateCableTypeOptionsWithValidation();
            }
        }
        public int MaxTechProductTemp
        {
            get => maxTechProductTemp;
            set
            {
                if (value < -60 || value > 1000)
                {
                    MessageBox.Show("Макс. техн. температура продукта должна быть в диапазоне от -60 до 1000", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                maxTechProductTemp = value;
                OnPropertyChanged(nameof(MaxTechProductTemp));
                UpdateCableTypeOptionsWithValidation();
            }
        }

        public int PhaseVoltage
        {
            get => Convert.ToInt32(phaseVoltage);
            set
            {
                if (value < 0 || value > 660)
                {
                    MessageBox.Show("Фазное напряжение должно быть в диапазоне от 0 до 660", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (numberCores == 2 && value > 600)
                {
                    MessageBox.Show("Максимальное допустимое напряжение для 2-х жильного кабеля 600 В", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                phaseVoltage = value;
                OnPropertyChanged(nameof(PhaseVoltage));
                OnPropertyChanged(nameof(WorkLoad));
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
                if (numberCores == value) return;

                if (phaseVoltage > 600 && value == 2)
                {
                    PhaseVoltage = Convert.ToInt32(600);
                    OnPropertyChanged(nameof(PhaseVoltage));
                    numberCores = value;
                    OnPropertyChanged(nameof(NumberCores));
                    MessageBox.Show("При фазном напряжении свыше 600В нельзя выбрать 2 жилы",
                        "Ошибка выбора", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                numberCores = value;
                OnPropertyChanged(nameof(NumberCores));
                PhaseVoltage = Convert.ToInt32(phaseVoltage);
                LineVoltage = Convert.ToInt32(lineVoltage);
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
                if (cableType == value) return;

                if (!ValidateCableForCurrentParameters(value))
                {
                    MessageBox.Show("Выбранный кабель недопустим при текущих параметрах", "Ошибка выбора", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                cableType = value;
                OnPropertyChanged(nameof(CableType));
            }
        }

        public int MinTempOn
        {
            get => minTempOn;
            set
            {
                if (value < -60 || value > 650)
                {
                    MessageBox.Show("Введите значение от -60 °С до +650 °С", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                minTempOn = value;
                OnPropertyChanged(nameof(MinTempOn));
            }
        }

        public string ConnectionScheme
        {
            get => connectionScheme;
            set
            {
                if (value == "линия" && LineVoltage > 600)
                {
                    PhaseVoltage = 220;
                    MessageBox.Show("Максимальное допустимое напряжение для 2-х жильного кабеля 600 В");
                }
                connectionScheme = value;
                OnPropertyChanged(nameof(ConnectionScheme));
                CheckLineVoltageLimit();
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
                CheckLineVoltageLimit();
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
                    _workEnvironment = value;
                    OnPropertyChanged(nameof(WorkEnvironment));
                    UpdateCableTypeOptionsWithValidation();
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
            }
        }



        public List<string> WorkEnvironmentOptions { get; } = new List<string> { "серная кислота", "соляная кислота", "плавиковая кислота", "фосфорная кислота", "азотная кислота", "органические кислоты", "щелочи", "соли", "морская вода", "хлориды", "-" };
        //public List<string> CableTypeOptions { get; } = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
        public List<string> NutritionOptions { get; private set; } = new List<string>() { "однофазное", "двухфазное", "трехфазное" };
        public List<string> ConnectionSchemeOptions { get; private set; } = new List<string>() {  "петля", "две петли", "три петли", "линия" };


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
            // Для схемы "линия" всегда 2 жилы, для остальных - 1
            int newCores = connectionScheme == "линия" ? 2 : 1;

            // Меняем только если значение действительно изменилось
            if (NumberCores != newCores)
            {
                NumberCores = newCores;
            }
        }


        private void UpdateCableTypeOptionsWithValidation()
        {
            var newOptions = FilterCablesByEnvironment(WorkEnvironment);
            newOptions = FilterCablesByTemperature(newOptions, SupportedTemp, MaxTechProductTemp);

            CableTypeOptions = newOptions;

            // Проверяем, что текущий выбранный кабель допустим
            if (!newOptions.Contains(CableType))
            {
                if (newOptions.Count > 0)
                {
                    CableType = newOptions.First();
                }
                else
                {
                    MessageBox.Show("Нет допустимых кабелей для указанных параметров", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private List<string> FilterCablesByEnvironment(string environment)
        {
            switch (environment)
            {
                case "серная кислота":
                    return new List<string> { "КНМСин", "КНМС825" };
                case "соляная кислота":
                    return new List<string> { "КНММН", "КНМСин", "КНМС825" };
                case "плавиковая кислота":
                case "фосфорная кислота":
                    return _allCableTypes.Where(t => t != "КНМС").ToList();
                case "азотная кислота":
                    return _allCableTypes.Where(t => t != "КНММ").ToList();
                case "морская вода":
                    return _allCableTypes.Where(t => t != "КНММ" && t != "КНМС").ToList();
                case "хлориды":
                    return _allCableTypes.Where(t => t != "КНМС").ToList();
                default:
                    return new List<string>(_allCableTypes);
            }
        }
        private List<string> FilterCablesByTemperature(List<string> cables, int supportedTemp, int maxTechTemp)
        {
            var result = new List<string>();

            foreach (var cable in cables)
            {
                if (ValidateCableTemperature(cable, supportedTemp, maxTechTemp))
                {
                    result.Add(cable);
                }
            }

            return result;
        }
        private bool ValidateCableForCurrentParameters(string cable)
        {
            if (!ValidateCableEnvironment(cable, WorkEnvironment))
                return false;

            return ValidateCableTemperature(cable, SupportedTemp, MaxTechProductTemp);
        }
        private bool ValidateCableEnvironment(string cable, string environment)
        {
            switch (environment)
            {
                case "серная кислота":
                    return cable == "КНМСин" || cable == "КНМС825";
                case "соляная кислота":
                    return cable == "КНММН" || cable == "КНМСин" || cable == "КНМС825";
                case "плавиковая кислота":
                case "фосфорная кислота":
                    return cable != "КНМС";
                case "азотная кислота":
                    return cable != "КНММ";
                case "морская вода":
                    return cable != "КНММ" && cable != "КНМС";
                case "хлориды":
                    return cable != "КНМС";
                default:
                    return true;
            }
        }
        private bool ValidateCableTemperature(string cable, int supportedTemp, int maxTechTemp)
        {
            switch (cable)
            {
                case "КНММ":
                    return supportedTemp <= 200 && maxTechTemp <= 250;
                case "КНММН":
                    return supportedTemp <= 400 && maxTechTemp <= 600;
                case "КНМС":
                    return supportedTemp <= 600 && maxTechTemp <= 800;
                case "КНМСин":
                    return supportedTemp <= 600 && maxTechTemp <= 1000;
                case "КНМС825":
                    return supportedTemp <= 650 && maxTechTemp <= 800;
                default:
                    return false;
            }
        }

        private void CheckLineVoltageLimit()
        {
            // Если выбрана схема "линия" и Workload превышает 600V
            if (ConnectionScheme == "линия" && WorkLoad > 600)
            {
                // Сбрасываем напряжение на 220V
                PhaseVoltage = 220;

                MessageBox.Show("Максимальное допустимое напряжение для 2-х жильного кабеля 600 В",
                    "Ошибка напряжения", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
