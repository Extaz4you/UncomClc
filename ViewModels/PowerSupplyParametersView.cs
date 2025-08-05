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
        private const double Sqrt3 = 1.73205080757;
        private double phaseVoltage = 220;
        private double lineVoltage = (220 * Sqrt3);
        private int current = 25;
        private int numberCores = 1;
        private bool isRecalculating = false;

        public double PhaseVoltage
        {
            get => phaseVoltage;
            set
            {
                if(numberCores == 1 && (value < 1 || value > 660))
                {
                    MessageBox.Show("Фазовое напряжение должно быть в диапазоне от 1 до 660", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (numberCores == 2 && (value < 1 || value > 600))
                {
                    MessageBox.Show("Фазовое напряжение должно быть в диапазоне от 1 до 600", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                phaseVoltage = value;
                OnPropertyChanged(nameof(PhaseVoltage));
                if (!isRecalculating)
                {
                    isRecalculating = true;
                    LineVoltage = value * Sqrt3;
                    isRecalculating = false;
                }
            }
        }

        public double LineVoltage
        {
            get => lineVoltage;
            set
            {
                if (numberCores == 1 && (value < 1 || value > 1143))
                {
                    MessageBox.Show("Линейное напряжение должно быть в диапазоне от 1 до 660", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (numberCores == 2 && (value < 1 || value > 1139))
                {
                    MessageBox.Show("Линейное напряжение должно быть в диапазоне от 1 до 600", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                lineVoltage = value;
                OnPropertyChanged(nameof(LineVoltage));
                if (!isRecalculating)
                {
                    isRecalculating = true;
                    PhaseVoltage = value / Sqrt3;
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
                numberCores = value;
                OnPropertyChanged(nameof(NumberCores));
                PhaseVoltage = phaseVoltage;
                LineVoltage = lineVoltage;
            }
        }

        public List<int> NumberCoresOptions { get; } = new List<int>() { 1, 2};
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
