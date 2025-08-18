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
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            TriggerAutoSave();
        }

        protected void TriggerAutoSave()
        {
            if (Application.Current?.MainWindow?.DataContext is MainViewModel mainVm)
            {
                mainVm.SaveCurrentParameters();
                mainVm.SaveToTempFile();
            }
        }
    }
}
