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
    public class ProcessView : INotifyPropertyChanged
    {
        private int supportedTemp = 5;
        private int maxTechProductTemp = 20;
        private int maxAddProductTemp = 100;
        private string steamingStatus = "Нет";
        public bool IsSteamingTemperatureEnabled => SteamingStatus == "Есть";
        public bool IsTemperatureClassEnabled => TemperatureClass != "-";
        private int steamingTemperature = 200;
        private string temperatureClass = "-";
        private int temperatureClassValue = 0;

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
                OnPropertyChanged(nameof(SupportedTemp));
            }
        }
        public int MaxAddProductTemp
        {
            get => maxAddProductTemp;
            set
            {
                if (value < -60 || value > 1000)
                {
                    MessageBox.Show("Макс. допус. температура продукта должна быть в диапазоне от -60 до 650", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                maxAddProductTemp = value;
                OnPropertyChanged(nameof(SupportedTemp));
            }
        }
        public string SteamingStatus
        {
            get => steamingStatus;
            set
            {
                steamingStatus = value;
                OnPropertyChanged(nameof(SteamingStatus));
                OnPropertyChanged(nameof(IsSteamingTemperatureEnabled));
            }
        }
        public int StreamingTemperature
        {
            get => steamingTemperature;
            set
            {
                if (value < 0 || value > 250)
                {
                    MessageBox.Show("Температура пропарки должна быть в диапазоне от 0 до 250", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                steamingTemperature = value;
                OnPropertyChanged(nameof(StreamingTemperature));
            }
        }
        public string TemperatureClass
        {
            get => temperatureClass;
            set
            {
                if(temperatureClass != value)
                {
                    temperatureClass = value;
                    TemperatureClassValue = GetTemperatureForClass(value);
                    OnPropertyChanged(nameof(TemperatureClass));
                    OnPropertyChanged(nameof(IsTemperatureClassEnabled));
                }
            }
        }
        public int TemperatureClassValue
        {
            get => temperatureClassValue;
            set
            {
                temperatureClassValue = value;
                OnPropertyChanged(nameof(TemperatureClassValue));
            }
        }



        public List<string> SteamingOptions { get; } = new List<string> { "Есть", "Нет" };
        public List<string> TemperatureClassOptions { get; } = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "-" };

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int GetTemperatureForClass(string tempClass)
        {
            switch (tempClass)
            {
                case "T1": return 450;
                case "T2": return 300;
                case "T3": return 200;
                case "T4": return 135;
                case "T5": return 100;
                case "T6": return 85;
                default: return 0;
            }
        }
    }

}
