using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace UncomClc.ViewModels
{
    public class ProcessView : INotifyPropertyChanged
    {

        private int maxAddProductTemp = 100;
        private string steamingStatus = "Нет";
        public bool IsSteamingTemperatureEnabled => SteamingStatus == "Есть";
        public bool IsTemperatureClassEnabled => TemperatureClass != "-";
        public bool IsIsolationThicknessEnabled => ThermalIsolation2 != "-";
        private int steamingTemperature = 200;
        private string temperatureClass = "-";
        private int temperatureClassValue = 0;
        private string pipe = "сталь углеродистая";
        private int diam = 57;
        private int thickness = 3;
        private float pipeKoef = 1.01f;
        private int lenght = 10;
        private string thermalIsolation = "минеральная вата";
        private string thermalIsolation2 = "-";
        private int isolationThickness = 50;
        private int isolationThickness2 = 50;


        public string Pipe
        {
            get => pipe;
            set
            {
                pipe = value;
                OnPropertyChanged(nameof(Pipe));
            }
        }
        public int Diam
        {
            get => diam;
            set
            {
                diam = value;
                OnPropertyChanged(nameof(Diam));
            }
        }
        public int Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                OnPropertyChanged(nameof(Thickness));
            }
        }
        public float PipeKoef
        {
            get => pipeKoef;
            set
            {
                pipeKoef = value;
                OnPropertyChanged(nameof(PipeKoef));
            }
        }
        public int Lenght
        {
            get => lenght;
            set
            {
                lenght = value;
                OnPropertyChanged(nameof(Lenght));
            }
        }


        public string ThermalIsolation
        {
            get => thermalIsolation;
            set
            {
                thermalIsolation = value;
                OnPropertyChanged(nameof(ThermalIsolation));
            }
        }
        public string ThermalIsolation2
        {
            get => thermalIsolation2;
            set
            {
                thermalIsolation2 = value;
                OnPropertyChanged(nameof(ThermalIsolation2));
                OnPropertyChanged(nameof(IsIsolationThicknessEnabled));
            }
        }

        public int IsolationThickness
        {
            get => isolationThickness;
            set
            {
                isolationThickness = value;
                OnPropertyChanged(nameof(IsolationThickness));
            }
        }
        public int IsolationThickness2
        {
            get => isolationThickness2;
            set
            {
                isolationThickness2 = value;
                OnPropertyChanged(nameof(IsolationThickness2));
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
                OnPropertyChanged(nameof(MaxAddProductTemp));
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
