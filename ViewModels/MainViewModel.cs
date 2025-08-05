using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomClc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ProcessView ProcessVM { get; } = new ProcessView();
        public EnvironmentView EnvironmentVM { get; } = new EnvironmentView();
        public PowerSupplyParametersView PowerSupplyParametersVM { get; } = new PowerSupplyParametersView();


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
