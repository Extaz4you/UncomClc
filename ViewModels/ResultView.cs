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

        private double _heatCableLength;
        public double HeatCableLength
        {
            get => _heatCableLength;
            set
            {
                _heatCableLength = value;
                OnPropertyChanged(nameof(HeatCableLength));
            }
        }

        private double _lobsh;
        public double Lobsh
        {
            get => _lobsh;
            set
            {
                _lobsh = value;
                OnPropertyChanged(nameof(Lobsh));
            }
        }

        private double _lzap;
        public double Lzap
        {
            get => _lzap;
            set
            {
                _lzap = value;
                OnPropertyChanged(nameof(Lzap));
            }
        }

        private double _lzadv;
        public double Lzadv
        {
            get => _lzadv;
            set
            {
                _lzadv = value;
                OnPropertyChanged(nameof(Lzadv));
            }
        }

        private double _lfl;
        public double Lfl
        {
            get => _lfl;
            set
            {
                _lfl = value;
                OnPropertyChanged(nameof(Lfl));
            }
        }

        private double _lop;
        public double Lop
        {
            get => _lop;
            set
            {
                _lop = value;
                OnPropertyChanged(nameof(Lop));
            }
        }

        private double _pobogr;
        public double Pobogr
        {
            get => _pobogr;
            set
            {
                _pobogr = value;
                OnPropertyChanged(nameof(Pobogr));
            }
        }

        private double _pkabrab;
        public double Pkabrab
        {
            get => _pkabrab;
            set
            {
                _pkabrab = value;
                OnPropertyChanged(nameof(Pkabrab));
            }
        }

        private string _scheme;
        public string Scheme
        {
            get => _scheme;
            set
            {
                _scheme = value;
                OnPropertyChanged(nameof(Scheme));
            }
        }

        private double _ssec;
        public double Ssec
        {
            get => _ssec;
            set
            {
                _ssec = value;
                OnPropertyChanged(nameof(Ssec));
            }
        }

        private double _tobol;
        public double Tobol
        {
            get => _tobol;
            set
            {
                _tobol = value;
                OnPropertyChanged(nameof(Tobol));
            }
        }

        private string _ch;
        public string CH
        {
            get => _ch;
            set
            {
                _ch = value;
                OnPropertyChanged(nameof(CH));
            }
        }

        private string _mark;
        public string Mark
        {
            get => _mark;
            set
            {
                _mark = value;
                OnPropertyChanged(nameof(Mark));
            }
        }

        private decimal _cross;
        public decimal Cross
        {
            get => _cross;
            set
            {
                _cross = value;
                OnPropertyChanged(nameof(Cross));
            }
        }

        private double _resistance;
        public double Resistance
        {
            get => _resistance;
            set
            {
                _resistance = value;
                OnPropertyChanged(nameof(Resistance));
            }
        }

        private decimal _urab;
        public decimal Urab
        {
            get => _urab;
            set
            {
                _urab = value;
                OnPropertyChanged(nameof(Urab));
            }
        }

        private double _psec20;
        public double Psec20
        {
            get => _psec20;
            set
            {
                _psec20 = value;
                OnPropertyChanged(nameof(Psec20));
            }
        }

        private double _lsec;
        public double Lsec
        {
            get => _lsec;
            set
            {
                _lsec = value;
                OnPropertyChanged(nameof(Lsec));
            }
        }

        private decimal _lust;
        public decimal Lust
        {
            get => _lust;
            set
            {
                _lust = value;
                OnPropertyChanged(nameof(Lust));
            }
        }

        private string _tempClass;
        public string TempClass
        {
            get => _tempClass;
            set
            {
                _tempClass = value;
                OnPropertyChanged(nameof(TempClass));
            }
        }

        private string _pit;
        public string Pit
        {
            get => _pit;
            set
            {
                _pit = value;
                OnPropertyChanged(nameof(Pit));
            }
        }

        private double _ivklmin;
        public double Ivklmin
        {
            get => _ivklmin;
            set
            {
                _ivklmin = value;
                OnPropertyChanged(nameof(Ivklmin));
            }
        }

        private double _irab;
        public double Irab
        {
            get => _irab;
            set
            {
                _irab = value;
                OnPropertyChanged(nameof(Irab));
            }
        }

        private decimal _psecvklmin;
        public decimal Psecvklmin
        {
            get => _psecvklmin;
            set
            {
                _psecvklmin = value;
                OnPropertyChanged(nameof(Psecvklmin));
            }
        }

        private decimal _psecrab;
        public decimal Psecrab
        {
            get => _psecrab;
            set
            {
                _psecrab = value;
                OnPropertyChanged(nameof(Psecrab));
            }
        }

        private bool isShellTemp;
        public bool IsShellTemp
        {
            get => isShellTemp;
            set
            {
                isShellTemp = value;
                OnPropertyChanged(nameof(IsShellTemp));
            }
        }
        private bool isStartCurrent;
        public bool IsStartCurrent
        {
            get => isStartCurrent;
            set
            {
                isStartCurrent = value;
                OnPropertyChanged(nameof(IsStartCurrent));
            }
        }
        private bool isLenght;
        public bool IsLenght
        {
            get => isLenght;
            set
            {
                isLenght = value;
                OnPropertyChanged(nameof(IsLenght));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
