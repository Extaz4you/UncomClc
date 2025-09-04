using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomClc.Models.Insulations;

namespace UncomClc.Models
{
    public class GeneralStructure : INotifyPropertyChanged
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public Parameters Parameters { get; set; }
        public CalculateResult CalculateResult { get; set; }

        private bool _hasWarning;
        public bool HasWarning
        {
            get => _hasWarning;
            set
            {
                _hasWarning = value;
                OnPropertyChanged(nameof(HasWarning));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class Parameters
    {
        public Pipe Pipe { get; set; }
        public int Diam { get; set; }
        public int Thickness { get; set; }
        public float PipeKoef { get; set; }
        public int Lenght { get; set; }
        public Insulation ThermalIsolation { get; set; }
        public Insulation ThermalIsolation2 { get; set; }
        public int IsolationThickness { get; set; }
        public int IsolationThickness2 { get; set; }
        public int MaxAddProductTemp { get; set; }
        public string SteamingStatus { get; set; }
        public int StreamingTemperature { get; set; }
        public int TemperatureClassValue { get; set; }
        public string TemperatureClass { get; set; }
        public int SupportCount { get; set; }
        public float SupportLenght { get; set; }
        public int ValveCount { get; set; }
        public float ValveLenght { get; set; }
        public int FlangCount { get; set; }
        public float FlangLength { get; set; }
        public int MaxEnvironmentTemp { get; set; }
        public int MinEnvironmentTemp { get; set; }
        public string PipelinePlacement { get; set; }
        public int SupportedTemp { get; set; }
        public int MaxTechProductTemp { get; set; }
        public int LineVoltage { get; set; }
        public int PhaseVoltage { get; set; }
        public int Current { get; set; }
        public int WorkLoad { get; set; }
        public int NumberCores { get; set; }
        public int LenghtSection { get; set; }
        public string CableType { get; set; }
        public int MinTempOn { get; set; }
        public string ConnectionScheme { get; set; }
        public string Nutrition { get; set; }
        public string WorkEnvironment { get; set; }
    }
    public class CalculateResult : INotifyPropertyChanged
    {
        public double Rpot { get; set; }
        public double HeatCableLenght { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
