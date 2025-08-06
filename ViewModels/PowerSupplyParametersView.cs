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


        public List<string> CableTypeOptions { get; } = new List<string>() { "КНММ", "КНММН", "КНМС", "КНМСин", "КНМС825" };
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
                newOptions.Add(1);
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
    }
}
