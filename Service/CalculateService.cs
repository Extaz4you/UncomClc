using OfficeOpenXml;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using UncomClc.Models;
using UncomClc.Models.Cable;
using UncomClc.ViewModels;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace UncomClc.Service
{
    public class CalculateService
    {
        private TextBlock TextBlock;

        public CalculateService()
        {
        }

        public CalculateResult Calculation(GeneralStructure structure, bool showMessage = true)
        {
            if (structure == null) return new CalculateResult();
            if (structure.Parameters.Diam < 2 * structure.Parameters.Thickness)
            {
                MessageBox.Show("Диаметр трубы не может быть меньше двух толщин стенки", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return new CalculateResult();
            }
            var param = structure.Parameters;

            int Dtr = param.Diam;
            double Dtr_m = param.Diam / 1000.0;
            int Tst = param.Thickness;
            double Tst_m = param.Thickness / 1000.0;
            int Ltr = param.Lenght;
            float KLtr = param.PipeKoef;
            int Tiz1 = param.IsolationThickness;
            double Tiz1_m = param.IsolationThickness / 1000.0;
            int Tiz2 = param.IsolationThickness2;
            double Tiz2_m = param.IsolationThickness2 / 1000.0;
            int Tokrmin = param.MinEnvironmentTemp;
            int Tokrmax = param.MaxEnvironmentTemp;
            int Ttr = param.SupportedTemp;
            int Ttechmax = param.MaxTechProductTemp;
            int Taddmax = param.MaxAddProductTemp;
            int Uf = param.PhaseVoltage;
            int Ul = param.LineVoltage;
            int Iabnom = param.Current;
            int Urab = param.WorkLoad;
            int Tvklmin = param.MinTempOn;
            int Szhil = param.NumberCores;
            int Lust = param.LenghtSection;
            int Szadv = param.ValveCount;
            int Sop = param.SupportCount;
            int Sfl = param.FlangCount;
            float Izadv = param.ValveLenght;
            float Iop = param.SupportLenght;
            float Ifl = param.FlangLength;
            string MrBd = param.CableType;
            float Ktr = param.Pipe.Koef;
            float Kiz = (float)param.ThermalIsolation.Koef;
            float Kiz2 = param.ThermalIsolation2 == null ? 0 : (float)param.ThermalIsolation2.Koef;
            int a = param.PipelinePlacement == "открытый воздух" ? 30 : 10;
            float Kzap = param.Diam >= 100 ? 1.1f : 1.15f;
            int Tpar = param.StreamingTemperature;
            string Tclass = param.TemperatureClass;
            int Tvalue = param.TemperatureClassValue;
            var bd = FindOutDataBase(param);


            // Расчет длин
            var lengths = CalculateLengths(param);

            //Общая длина
            var Lobsh = CalculateTotalLength(lengths);

            // Расчет теплопотерь
            double rpot = CalculateHeatLoss(param, Lobsh);

            //Подбор бд и выбор 1 кабеля
            var cables = ExcelReader.ReadCableDataFromExcel(bd);
            if (cables == null || !cables.Any())
            {
                MessageBox.Show("Не найдены кабели в базе данных", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return new CalculateResult();
            }


            var maxRow = cables.Max(x => x.RowNumber);
            var iteration = 0;
            double Pobogrrab = 0;
            bool cableFound = false;
            CableModel selectedCable = null;
            double Lsec;
            double Rsec20;
            (double Rsecrab, decimal Psecrab, double Pkabrab, decimal Tkabrab0, int Tkabrab, int iteration) result;
            do
            {
                iteration++;
                var findNeededCable = cables.FirstOrDefault(x => x.RowNumber == iteration);
                findNeededCable.Resistance = findNeededCable.Resistance / 1000.0;

                Lsec = Lobsh;
                if (param.ConnectionScheme == "петля" || param.ConnectionScheme == "две петли" || param.ConnectionScheme == "три петли")
                    Lsec = 2 * Lobsh;

                Rsec20 = findNeededCable.Resistance * Lsec;

                var Tkabrab = Ttr;

                result = CalculateCableTemperatureIterative(Rsec20, Urab, param.ConnectionScheme, Lsec, findNeededCable, Ttr);


                Pobogrrab = CalculatePobogr(result.Pkabrab, param);
                if (Pobogrrab > rpot)
                {
                    cableFound = true;
                    selectedCable = findNeededCable;
                    break;
                }

                // Проверяем, не превысили ли максимальный номер строки
                if (iteration >= maxRow)
                {
                    break;
                }
            }
            while (iteration <= maxRow);


            // Проверяем, найден ли подходящий кабель
            if (!cableFound)
            {
                if (showMessage) ShowWarningMessage(1);
                structure.HasWarning = true;
                return new CalculateResult();
            }


            double Rsecvklmin = Rsec20 * (1 + double.Parse(selectedCable.Alfa) * (Tvklmin - 20));

            var caclRes = CalculateResistance(param, Rsec20, Urab, Rsecvklmin, result.Rsecrab);

            var shellTemp = CalculateShellTemperature(Tokrmax, Rsec20, Urab, Lsec, param.ConnectionScheme, selectedCable, param);

            var CH = $"CH-{selectedCable.Mark} {selectedCable.Cross}-R{selectedCable.Resistance * 1000}-U{Urab}-P{Math.Round(caclRes.Psec20 / 1000, 2, MidpointRounding.AwayFromZero)}-L{Lsec.ToString("N1")}/{Lust.ToString("N1")}";
            if (Tvalue > 0) CH += $"-{Tclass}";
            CH += " ТУ 16.К03-76-2018";

            var finalResult = new CalculateResult
            {
                Rpot = rpot,
                Lobsh = Lobsh,
                Lzap = lengths.Lzap,
                Lzadv = lengths.Lzadv,
                Lfl = lengths.Lfl,
                Lop = lengths.Lop,
                Pobogr = Pobogrrab,
                Pkabrab = result.Pkabrab,
                Scheme = structure.Parameters.ConnectionScheme,
                Ssec = (int)caclRes.ssec,
                Lsec = Lsec,
                Lust = Lust,
                TempClass = structure.Parameters.TemperatureClass,
                Pit = structure.Parameters.Nutrition,
                Urab = Urab,
                Psec20 = Math.Round(caclRes.Psec20 / 1000, 2, MidpointRounding.AwayFromZero),
                Ivklmin = caclRes.Ivklmin,
                Irab = caclRes.Irab,
                Psecvklmin = (decimal)caclRes.Psecvklmin / 1000,
                Psecrab = (decimal)result.Psecrab / 1000,
                HeatCableLenght = Lsec,
                CH = CH,
                Mark = selectedCable.Mark,
                Cross = decimal.Parse(selectedCable.Cross),
                Resistance = selectedCable.Resistance,
                Tobol = shellTemp.Tobol
            };

            var maxTemp = GetmaxTempFromBd(bd);
            structure.HasWarning = false;


            if (double.Parse(selectedCable.Length) < Lsec)
            {
                if (showMessage) ShowWarningMessage(2);
                finalResult.IsLenght = true;
                structure.HasWarning = true;
            }
            if (shellTemp.Tobol > maxTemp)
            {
                if (showMessage) ShowWarningMessage(3);
                finalResult.IsShellTemp = true;
                structure.HasWarning = true;
            }
            if (shellTemp.Tobol > Taddmax)
            {
                if (showMessage) ShowWarningMessage(4);
                finalResult.IsShellTemp = true;
                structure.HasWarning = true;
            }
            if (Tvalue > 0 && shellTemp.Tobol > Tvalue)
            {
                if (showMessage) ShowWarningMessage(5);
                finalResult.IsShellTemp = true;
                structure.HasWarning = true;
            }
            if (caclRes.Ivklmin > Iabnom)
            {
                if (showMessage) ShowWarningMessage(6);
                finalResult.IsStartCurrent = true;
                structure.HasWarning = true;
            }
            structure.SuccessCalculation = true;

            return finalResult;
        }

        public string FindOutDataBase(Parameters param)
        {
            if (param.NumberCores == 1)
            {
                if (param.CableType == "КНММ") return "КНММ";
                if (param.CableType == "КНММН") return "КНММН";
                if (param.CableType == "КНМС") return "КНМС";
                if (param.CableType == "КНМСин") return "КНМСин";
                if (param.CableType == "КНМС825") return "КНМС825";
            }
            if (param.NumberCores == 2)
            {
                if (param.CableType == "КНММ" && param.WorkLoad <= 300) return "2КНММ-В3";
                if (param.CableType == "КНММН" && param.WorkLoad <= 300) return "2КНММН-В3";
                if (param.CableType == "КНМС" && param.WorkLoad <= 300) return "2КНМС-В3";
                if (param.CableType == "КНМСин" && param.WorkLoad <= 300) return "2КНМСин-В3";
                if (param.CableType == "КНМС825" && param.WorkLoad <= 300) return "2КНМС825-В3";

                if (param.CableType == "КНММ" && param.WorkLoad > 300) return "2КНММ-В6";
                if (param.CableType == "КНММН" && param.WorkLoad > 300) return "2КНММН-В6";
                if (param.CableType == "КНМС" && param.WorkLoad > 300) return "2КНМС-В6";
                if (param.CableType == "КНМСин" && param.WorkLoad > 300) return "2КНМСин-В6";
                if (param.CableType == "КНМС825" && param.WorkLoad > 300) return "2КНМС825-В6";
            }
            return "";
        }

        // Вспомогательные методы
        private Lengths CalculateLengths(Parameters param)
        {
            return new Lengths
            {
                Ltr = param.Lenght,
                Lzap = param.Lenght * param.PipeKoef - param.Lenght,
                Lzadv = param.ValveCount * param.ValveLenght,
                Lfl = param.FlangCount * param.FlangLength,
                Lop = param.SupportCount * param.SupportLenght
            };
        }

        private double CalculateTotalLength(Lengths lengths)
        {
            return lengths.Lzap + lengths.Lzadv + lengths.Lfl + lengths.Lop + lengths.Ltr;
        }

        private double CalculateHeatLoss(Parameters param, double Lobsh)
        {
            double Dtr_m = param.Diam / 1000.0;
            double Tst_m = param.Thickness / 1000.0;
            double Tiz1_m = param.IsolationThickness / 1000.0;
            double Tiz2_m = param.IsolationThickness2 / 1000.0;

            float Ktr = param.Pipe.Koef;
            float Kiz = (float)param.ThermalIsolation.Koef;
            float Kiz2 = param.ThermalIsolation2 == null ? 0 : (float)param.ThermalIsolation2.Koef;
            int a = param.PipelinePlacement == "открытый воздух" ? 30 : 10;
            float Kzap = param.Diam >= 100 ? 1.1f : 1.15f;

            if (param.ThermalIsolation2 != null && param.ThermalIsolation2.Name != "-")
            {
                return Kzap * (param.SupportedTemp - param.MinEnvironmentTemp) / (
                    Math.Log(Dtr_m / (Dtr_m - 2 * Tst_m)) / (2 * Math.PI * Ktr) +
                    Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Math.PI * Kiz) +
                    Math.Log((Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) / (Dtr_m + 2 * Tiz1_m)) / (2 * Math.PI * Kiz2) +
                    1 / (Math.PI * (Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) * a)
                );
            }
            else
            {
                return Kzap * (param.SupportedTemp - param.MinEnvironmentTemp) / (
                    Math.Log(Dtr_m / (Dtr_m - 2 * Tst_m)) / (2 * Math.PI * Ktr) +
                    Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Math.PI * Kiz) +
                    1 / (Math.PI * (Dtr_m + 2 * Tiz1_m) * a)
                );
            }
        }

        private (double Rsecrab, decimal Psecrab, double Pkabrab, decimal Tkabrab0, int Tkabrab, int iteration)
            CalculateCableTemperatureIterative(double Rsec20, int Urab, string connectionScheme, double Lsec,
                                              CableModel cable, int Ttr)
        {
            const int maxIterations = 10;
            int iteration = 0;
            int Tkabrab = Ttr; // Начальное значение температуры кабеля
            decimal Tkabrab0;
            decimal Psecrab;
            double Rsecrab, Pkabrab;

            bool converged = false;

            do
            {
                iteration++;

                // Вычисляем сопротивление при текущей температуре
                Rsecrab = Rsec20 * (1 + double.Parse(cable.Alfa) * (Tkabrab - 20));

                // Вычисляем мощность в зависимости от схемы подключения
                if (connectionScheme == "линия" || connectionScheme == "петля" || connectionScheme == "две петли" || connectionScheme == "три петли")
                {
                    Psecrab = (decimal)((Urab * Urab) / Rsecrab);
                }
                else
                {
                    Psecrab = (decimal)((Urab * Urab) / (3 * Rsecrab));
                }

                Pkabrab = (double)Psecrab / Lsec;
                Tkabrab0 = (decimal)Pkabrab / (60m * 3.14m * (cable.Dkab / 1000)) + Ttr;

                // Проверяем сходимость
                if (Math.Abs(Tkabrab0 - Tkabrab) <= 1m)
                {
                    converged = true;
                }
                else
                {
                    // Обновляем температуру для следующей итерации
                    Tkabrab = (int)Math.Round(Tkabrab0);
                }

            } while (!converged && iteration < maxIterations);

            return (Rsecrab, Psecrab, Pkabrab, Tkabrab0, Tkabrab, iteration);
        }

        private double CalculatePobogr(double Pkabrab, Parameters param)
        {
            switch (param.ConnectionScheme)
            {
                case "линия":
                    return Pkabrab * 1;
                case "петля":
                    return Pkabrab * 2;
                case "две петли":
                    return Pkabrab * 4;
                case "три петли":
                    return Pkabrab * 6;
                case "звезда":
                    return Pkabrab * 3;
                case "две звезды":
                    return Pkabrab * 6;
                case "три звезды":
                    return Pkabrab * 9;
                default:
                    return Pkabrab;
            }
        }

        private int HeatSection(string scheme)
        {
            switch (scheme)
            {
                case "линия":
                    return 1;
                case "петля":
                    return 1;
                case "две петли":
                    return 2;
                case "три петли":
                    return 3;
                case "звезда":
                    return 3;
                case "две звезды":
                    return 6;
                case "три звезды":
                    return 9;
                default:
                    return 0;
            }
        }

        private void ShowWarningMessage(int errorPlace)
        {
            switch (errorPlace)
            {
                case 1:
                    MessageBox.Show("Не удалось подобрать нагревательную секцию по необходимой мощности обогрева. Попробуйте изменить параметры КСЭО или питающей сети",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 2:
                    MessageBox.Show("Расчетная длина нагревательной секции превышает максимальную длину кабеля в бухте. Обратитесь к производителю нагревательных секций на предмет сращивания кабеля муфтами",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 3:
                    MessageBox.Show("Расчетная максимальная температура оболочки кабеля превышает максимальную допустимую температуру воздействия на кабель. Попробуйте изменить параметры КСЭО, марку кабеля или предусмотрите систему регулирования температуры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 4:
                    MessageBox.Show("Расчетная максимальная температура оболочки кабеля превышает максимальную допустимую температуру продукта. Попробуйте изменить параметры КСЭО или предусмотрите систему регулирования температуры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 5:
                    MessageBox.Show("Расчетная максимальная температура оболочки кабеля превышает температурный класс ВЗЭО. Попробуйте изменить параметры КСЭО или предусмотрите систему регулирования температуры",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 6:
                    MessageBox.Show("Стартовый ток секции превышает номинальный ток автоматического выключателя. Попробуйте изменить параметры КСЭО или предусмотреть другое защитное оборудование",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

        }

        private (double Psec20, decimal Psecvklmin, double ssec, double Ivklmin, double Irab)
            CalculateResistance(Parameters param, double Rsec20, double Urab, double Rsecvklmin, double Rsecrab)
        {
            double Psec20 = 0;
            decimal Psecvklmin = 0;

            if (param.ConnectionScheme == "линия" || param.ConnectionScheme == "петля" || param.ConnectionScheme == "две петли" || param.ConnectionScheme == "три петли")
            {
                Psec20 = Math.Pow(Urab, 2) / Rsec20;
                Psecvklmin = (decimal)(Math.Pow(Urab, 2) / Rsecvklmin);
            }
            else
            {
                Psec20 = Math.Pow(Urab, 2) / (3 * Rsec20);
                Psecvklmin = (decimal)(Math.Pow(Urab, 2) / (3 * Rsecvklmin));
            }
            var ssec = HeatSection(param.ConnectionScheme);
            var Ivklmin = Urab / Rsecvklmin;
            var Irab = Urab / Rsecrab;
            return (Psec20, Psecvklmin, ssec, Ivklmin, Irab);
        }

        private (double Rsecmax, double Psecmax, double Pkabmax, double Ttpmax, double Pobogmax, double iteration, double Tobol, double Tobol0)
            CalculateShellTemperature(int Tokrmax, double Rsec20, double Urab, double Lsec, string connectionScheme, CableModel cable, Parameters param)
        {
            double Tobol = Tokrmax;
            double Tobol0;
            bool converged = false;
            const int maxIterations = 10;
            int iteration = 0;
            double Ttpmax;
            double Rsecmax, Psecmax, Pkabmax, Pobogmax;

            double Dtr_m = param.Diam / 1000.0;
            double Tiz1_m = param.IsolationThickness / 1000.0;
            double Tiz2_m = param.IsolationThickness2 / 1000.0;
            int Ttr = param.SupportedTemp;
            float Kiz = (float)param.ThermalIsolation.Koef;
            float Kiz2 = param.ThermalIsolation2 == null ? 0 : (float)param.ThermalIsolation2.Koef;
            int a = param.PipelinePlacement == "открытый воздух" ? 30 : 10;

            do
            {
                iteration++;

                Rsecmax = Rsec20 * (1 + double.Parse(cable.Alfa) * (Tobol - 20));

                // Вычисляем мощность в зависимости от схемы подключения
                if (connectionScheme == "линия" || connectionScheme == "петля" || connectionScheme == "две петли" || connectionScheme == "три петли")
                {
                    Psecmax = Math.Pow(Urab * 1.1, 2) / Rsecmax;
                }
                else
                {
                    Psecmax = Math.Pow(Urab * 1.1, 2) / (3 * Rsecmax);
                }

                Pkabmax = Psecmax / Lsec;

                Pobogmax = CalculatePobogr(Pkabmax, param);

                if (param.ThermalIsolation2 != null && param.ThermalIsolation2.Name != "-" && Tiz2_m > 0)
                {
                    Ttpmax = (Pobogmax / 3.14) * (Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Kiz) + Math.Log((Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) / (Dtr_m + 2 * Tiz1_m)) / (2 * Kiz2) + 1 / ((Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) * a)) + Tokrmax;
                }
                else
                {
                    Ttpmax = (Pobogmax / 3.14) * (Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Kiz) + 1 / ((Dtr_m + 2 * Tiz1_m) * a)) + Tokrmax;
                }

                double alpha_cable = 60;//double.Parse(cable.Alfa);
                double D_kab_m = double.Parse(cable.Dkab.ToString()) / 1000.0;
                Tobol0 = Pkabmax / (alpha_cable * Math.PI * D_kab_m) + Ttpmax;

                // Проверяем сходимость
                if (Math.Abs(Tobol0 - Tobol) <= 1)
                {
                    converged = true;
                }
                else
                {
                    // Обновляем температуру для следующей итерации
                    Tobol = Tobol0;
                }
            }
            while (!converged && iteration < maxIterations);

            return (Rsecmax, Psecmax, Pkabmax, Ttpmax, Pobogmax, iteration, Tobol, Tobol0);
        }


        private int GetmaxTempFromBd(string bd)
        {
            var maxTemps = new Dictionary<string, int>
            {
                { "КНММ", 200 },
                { "КНММН", 400 },
                { "КНМС", 600 },
                { "КНМСин", 600 },
                { "КНМС825", 650 },
                { "2КНММ-В3", 200 },
                { "2КНММН-В3", 400 },
                { "2КНМС-В3", 600 },
                { "2КНМСин-В3", 600 },
                { "2КНМС825-В3", 650 },
                { "2КНММ-В6", 200 },
                { "2КНММН-В6", 400 },
                { "2КНМС-В6", 600 },
                { "2КНМСин-В6", 600 },
                { "2КНМС825-В6", 650 },
            };
            int temp = 0;
            if (maxTemps.ContainsKey(bd))
            {
                temp = maxTemps[bd];
            }
            return temp;
        }



        // Вспомогательные классы
        public class Lengths
        {
            public float Lzap { get; set; }
            public double Lzadv { get; set; }
            public double Lfl { get; set; }
            public double Lop { get; set; }
            public double Ltr { get; set; }
        }
    }
}

