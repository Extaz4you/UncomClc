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
    public class ProcessView : BaseViewModel
    {

        private int maxAddProductTemp = 100;
        private string steamingStatus = "Нет";
        public bool IsSteamingTemperatureEnabled => SteamingStatus == "Есть";
        public bool IsTemperatureClassEnabled => TemperatureClass != "-";
        public bool IsIsolationThicknessEnabled => ThermalIsolation2 != "- не выбрано -";
        private int steamingTemperature = 200;
        private string temperatureClass = "-";
        private int temperatureClassValue = 0;
        private string pipe = "сталь углеродистая";
        private int diam = 57;
        private int thickness = 3;
        private float pipeKoef = 1.01f;
        private int lenght = 10;
        private string thermalIsolation = "минеральная вата";
        private string previousThermalIsolation = "минеральная вата";
        private string thermalIsolation2 = "- не выбрано -";
        private int isolationThickness = 50;
        private int isolationThickness2 = 0;
        private int supportCount = 0;
        private float supportLength = 0;
        private int valveCount = 0;
        private float valveLength = 0;
        private int flangCount = 0;
        private float flangLength = 0;


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
                if (value > 1220)
                {
                    MessageBox.Show("Диамметр не может быть более 1220 мм");
                    return;
                }
                diam = value;
                OnPropertyChanged(nameof(Diam));
                ChangeLenghtByTable();
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
                if (string.IsNullOrEmpty(value) || value == "")
                {
                    MessageBox.Show("Нельзя выбрать пустое значение", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Восстанавливаем значение через Dispatcher чтобы UI успел обновиться
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        thermalIsolation = previousThermalIsolation;
                        OnPropertyChanged(nameof(ThermalIsolation));
                    }));

                    return;
                }

                if (thermalIsolation != value)
                {
                    previousThermalIsolation = thermalIsolation;
                    thermalIsolation = value;
                    OnPropertyChanged(nameof(ThermalIsolation));
                }
            }
        }
        public string ThermalIsolation2
        {
            get => thermalIsolation2;
            set
            {
                if (thermalIsolation2 != value)
                {
                    thermalIsolation2 = value;
                    if (value == "- не выбрано -")
                    {
                        IsolationThickness2 = 0;
                    }
                    OnPropertyChanged(nameof(ThermalIsolation2));
                    OnPropertyChanged(nameof(IsIsolationThicknessEnabled));
                }
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
                if (temperatureClass != value)
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

        public int SupportCount
        {
            get => supportCount;
            set
            {
                supportCount = value;
                OnPropertyChanged(nameof(SupportCount));
                ChangeLenghtByTable();
            }
        }
        public float SupportLenght
        {
            get => supportLength;
            set
            {
                supportLength = value;
                OnPropertyChanged(nameof(SupportLenght));
            }
        }
        public int ValveCount
        {
            get => valveCount;
            set
            {
                valveCount = value;
                OnPropertyChanged(nameof(ValveCount));
                ChangeLenghtByTable();
            }
        }
        public float ValveLenght
        {
            get => valveLength;
            set
            {
                valveLength = value;
                OnPropertyChanged(nameof(ValveLenght));
            }
        }
        public int FlangCount
        {
            get => flangCount;
            set
            {
                flangCount = value;
                OnPropertyChanged(nameof(FlangCount));
                ChangeLenghtByTable();
            }
        }
        public float FlangLength
        {
            get => flangLength;
            set
            {
                flangLength = value;
                OnPropertyChanged(nameof(FlangLength));
            }
        }


        public List<string> SteamingOptions { get; } = new List<string> { "Есть", "Нет" };
        public List<string> TemperatureClassOptions { get; } = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "-" };

        public Dictionary<int, SupportValveFlangTable> SupValFlTable { get; } = new Dictionary<int, SupportValveFlangTable>
        {
            {25, new SupportValveFlangTable() { FlLength = 0.3f, ValLength = 0.5f, SupLength = 0.6f } },
            {32, new SupportValveFlangTable() { FlLength = 0.3f, ValLength = 0.6f, SupLength = 0.6f } },
            {57, new SupportValveFlangTable() { FlLength = 0.4f, ValLength = 2.0f, SupLength = 0.7f } },
            {76, new SupportValveFlangTable() { FlLength = 0.4f, ValLength = 2.4f, SupLength = 0.7f } },
            {89, new SupportValveFlangTable() { FlLength = 0.5f, ValLength = 2.4f, SupLength = 0.7f } },
            {108, new SupportValveFlangTable() { FlLength = 0.6f, ValLength = 2.4f, SupLength = 0.8f } },
            {159, new SupportValveFlangTable() { FlLength = 0.6f, ValLength = 2.4f, SupLength = 0.8f } },
            {219, new SupportValveFlangTable() { FlLength = 1f, ValLength = 2.8f, SupLength = 0.8f } },
            {273, new SupportValveFlangTable() { FlLength = 1f, ValLength = 3.4f, SupLength = 0.8f } },
            {325, new SupportValveFlangTable() { FlLength = 1.3f, ValLength = 4.1f, SupLength = 1.2f } },
            {377, new SupportValveFlangTable() { FlLength = 1.3f, ValLength = 4.5f, SupLength = 1.2f } },
            {426, new SupportValveFlangTable() { FlLength = 1.3f, ValLength = 5.1f, SupLength = 1.2f } },
            {530, new SupportValveFlangTable() { FlLength = 1.5f, ValLength = 6.4f, SupLength = 1.2f } },
            {630, new SupportValveFlangTable() { FlLength = 1.5f, ValLength = 7.7f, SupLength = 1.5f } },
            {830, new SupportValveFlangTable() { FlLength = 2.2f, ValLength = 10f, SupLength = 2f } },
            {1020, new SupportValveFlangTable() { FlLength = 2.7f, ValLength = 12.3f, SupLength = 2.5f } },
            {1220, new SupportValveFlangTable() { FlLength = 3.1f, ValLength = 14.7f, SupLength = 3 } },
        };

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

        private void ChangeLenghtByTable()
        {
            var info = SupValFlTable
                       .Where(x => x.Key >= Diam)
                       .OrderBy(x => x.Key)
                       .Select(x => x.Value)
                       .FirstOrDefault();

            if(SupportCount > 0) SupportLenght = info.SupLength;
            if (ValveCount > 0) ValveLenght = info.ValLength;
            if (FlangCount > 0) FlangLength = info.FlLength;
        }
    }

}
