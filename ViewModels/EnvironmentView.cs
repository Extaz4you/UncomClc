using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UncomClc.ViewModels
{
    public class EnvironmentView : BaseViewModel
    {
        private int minEnvironmentTemp = -20;
        private int maxEnvironmentTemp = 30;
        private string pipelinePlacement = "открытый воздух";

        public int MaxEnvironmentTemp
        {
            get => maxEnvironmentTemp;
            set
            {
                maxEnvironmentTemp = value;
                OnPropertyChanged(nameof(MaxEnvironmentTemp));
            }
        }
        public int MinEnvironmentTemp
        {
            get => minEnvironmentTemp;
            set
            {
                if(value > MaxEnvironmentTemp)
                {
                    MessageBox.Show("Минимальная температура окружающей среды не может быть больше максимальной температуры окружающей среды", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                minEnvironmentTemp = value;
                OnPropertyChanged(nameof(MinEnvironmentTemp));
            }
        }
        public string PipelinePlacement
        {
            get => pipelinePlacement;
            set
            {
                pipelinePlacement = value;
                OnPropertyChanged(nameof(PipelinePlacement));
            }
        }


        public List<string> PipelinePlacementOptions { get; } = new List<string> { "открытый воздух", "в помещении" };

    }
}
