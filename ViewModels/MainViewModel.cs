using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UncomClc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ProcessView ProcessVM { get; } = new ProcessView();
        public EnvironmentView EnvironmentVM { get; } = new EnvironmentView();
        public PowerSupplyParametersView PowerSupplyParametersVM { get; } = new PowerSupplyParametersView();


        public ICommand AddPipeCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel()
        {
            // Инициализация команд
            AddPipeCommand = new RelayCommand(AddNewPipe);
            DeleteCommand = new RelayCommand(DeleteSelectedPipe);
            CreateCommand = new RelayCommand(CreateFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void AddNewPipe()
        {

        }
        private void DeleteSelectedPipe()
        {

        }
        private void CreateFile()
        {
  
        }
        private void OpenFile()
        {
           
        }
        private void SaveFile()
        {

        }
    }
}
