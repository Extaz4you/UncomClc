using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomClc.Models.Insulations;

namespace UncomClc.Models
{
    public class GeneralStructure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Parameters Parameters { get; set; }
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
        public float SupportCount { get; set; }
        public float SupportLenght { get; set; }
        public float ValveCount { get; set; }
        public float ValveLenght { get; set; }
        public float FlangCount { get; set; }
        public float FlangLength { get; set; }
        public int MaxEnvironmentTemp { get; set; }
        public int MinEnvironmentTemp { get; set; }
        public string PipelinePlacement { get; set; }
        public int SupportedTemp { get; set; }
        public int MaxTechProductTemp { get; set; }
        public int LineVoltage { get; set; }
        public int Current { get; set; }
        public int NumberCores { get; set; }
        public int LenghtSection { get; set; }
        public string CableType { get; set; }
        public int MinTempOn { get; set; }
        public string ConnectionScheme { get; set; }
        public string Nutrition { get; set; }
        public string WorkEnvironment { get; set; }
    }
}
