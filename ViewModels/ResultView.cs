using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomClc.ViewModels
{
    public class ResultView : INotifyPropertyChanged
    {
        private double _calculatedHeatLoss;

        public double CalculatedHeatLoss
        {
            get => _calculatedHeatLoss;
            set
            {
                _calculatedHeatLoss = value;
                OnPropertyChanged(nameof(CalculatedHeatLoss));
            }
        }






        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
