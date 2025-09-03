using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UncomClc.Models;
using UncomClc.Models.Cable;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace UncomClc.Service
{
    public class CalculateService
    {
        private TextBlock TextBlock;

        public CalculateService(TextBlock block)
        {
            TextBlock = block;
        }
        public CalculateResult Calculation(GeneralStructure structure)
        {
            if (structure == null) return new CalculateResult();
            if (structure.Parameters.Diam < 2 * structure.Parameters.Thickness)
            {
                MessageBox.Show("Диаметр трубы не может быть меньше двух толщин стенки", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return new CalculateResult();
            }
            TextBlock.Text = "";
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

            // Вывод переменных
            PrintVariables(param);

            // Расчет длин
            var lengths = CalculateLengths(param);
            PrintLengths(lengths);

            //Общая длина
            var Lobsh = CalculateTotalLength(lengths);
            TextBlock.Text += $"\r\nLobsh - {Lobsh}";

            // Расчет теплопотерь
            double rpot = CalculateHeatLoss(param, Lobsh);
            TextBlock.Text += $"\r\nТеплопотери: {rpot}\r\n";

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
            do
            {
                iteration++;
                var findNeededCable = cables.FirstOrDefault(x => x.RowNumber == iteration);

                findNeededCable.Resistance = findNeededCable.Resistance / 1000.0;
                TextBlock.Text += $"\r\n Элемент из БД ({bd}): Номер строки: {findNeededCable.RowNumber} Марка: {findNeededCable.Mark} Сечение: {findNeededCable.Cross} Сопротивление: {findNeededCable.Resistance} Альфа: {findNeededCable.Alfa} Дельта: {findNeededCable.Delta} Длина: {findNeededCable.Length}\r\n";


                var Lsec = Lobsh;
                if (param.ConnectionScheme == "петля" || param.ConnectionScheme == "две петли" || param.ConnectionScheme == "три петли")
                    Lsec = 2 * Lobsh;
                TextBlock.Text += $"\r\nLsec - {Lsec}\r\n";

                var Rsec20 = findNeededCable.Resistance * Lsec;
                TextBlock.Text += $"\r\nRsec20 - {Rsec20}\r\n";

                var Tkabrab = Ttr;
                TextBlock.Text += $"\r\nTkabrab - {Tkabrab}\r\n";

                var result = CalculateCableTemperatureIterative(Rsec20, Urab, param.ConnectionScheme, Lsec, findNeededCable, Ttr);
                TextBlock.Text += $"\r\nRsecrab - {result.Rsecrab}\r\n";
                TextBlock.Text += $"\r\nPsecrab - {result.Psecrab}\r\n";
                TextBlock.Text += $"\r\nPkabrab - {result.Pkabrab}\r\n";
                TextBlock.Text += $"\r\nTkabrab0 - {result.Tkabrab0}\r\n";
                TextBlock.Text += $"\r\nФинальный результат после {result.iteration} итераций: Tkabrab = {result.Tkabrab}°C";

                Pobogrrab = CalculatePobogr(result.Pkabrab, param);
                TextBlock.Text += $"\r\nPobogrrab - {Pobogrrab}\r\n";
                if (Pobogrrab > rpot)
                {
                    TextBlock.Text += $"\r\n✅ УСЛОВИЕ ВЫПОЛНЕНО: Pobogrrab ({Pobogrrab}) > rpot ({rpot})\r\n";
                    cableFound = true;
                    selectedCable = findNeededCable;
                    break; 
                }
                else
                {
                    TextBlock.Text += $"\r\n❌ УСЛОВИЕ НЕ ВЫПОЛНЕНО: Pobogrrab ({Pobogrrab}) <= rpot ({rpot})\r\n";
                    TextBlock.Text += $"\r\nПродолжаем поиск...\r\n";
                }

                // Проверяем, не превысили ли максимальный номер строки
                if (iteration >= maxRow)
                {
                    TextBlock.Text += $"\r\nДостигнут максимальный номер строки ({maxRow}). Поиск завершен.\r\n";
                    break;
                }
            }
            while (iteration <= maxRow);

            // Проверяем, найден ли подходящий кабель
            if (!cableFound)
            {
                MessageBox.Show("Не удалось подобрать подходящий кабель. Все кабели из базы данных не обеспечивают достаточную мощность.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new CalculateResult();
            }
            TextBlock.Text += $"\r\n\n🎉 ПОДОБРАН ПОДХОДЯЩИЙ КАБЕЛЬ:\r\n";
            TextBlock.Text += $"\r\nМарка: {selectedCable.Mark} Сопротивление: {selectedCable.Resistance} Номер строки: {selectedCable.RowNumber}";


            var finalResult = new CalculateResult { Rpot = rpot };
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
        private void PrintVariables(Parameters param)
        {
            TextBlock.Text += $"\r\nПЕРЕМЕННЫЕ\r\n";
            TextBlock.Text += $"\r\nDtr - {param.Diam / 1000.0}";
            TextBlock.Text += $"\r\nTst - {param.Thickness / 1000.0}";
            TextBlock.Text += $"\r\nLtr - {param.Lenght}";
            TextBlock.Text += $"\r\nKLtr - {param.PipeKoef}";
            TextBlock.Text += $"\r\nTclass/value  - {param.TemperatureClass} - {param.TemperatureClassValue}";
        }

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

        private void PrintLengths(Lengths lengths)
        {
            TextBlock.Text += $"\r\n1 РАСЧЕТ\r\n";
            TextBlock.Text += $"\r\nLzap - {lengths.Lzap}";
            TextBlock.Text += $"\r\nLzadv - {lengths.Lzadv}";
            TextBlock.Text += $"\r\nLfl - {lengths.Lfl}";
            TextBlock.Text += $"\r\nLop - {lengths.Lop}";
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

            if (param.ThermalIsolation2 != null && !string.IsNullOrEmpty(param.ThermalIsolation2.Name))
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

        private (double Rsecrab, double Psecrab, double Pkabrab, decimal Tkabrab0, int Tkabrab, int iteration)
            CalculateCableTemperatureIterative(double Rsec20, int Urab, string connectionScheme, double Lsec,
                                              CableModel cable, int Ttr)
        {
            int iteration = 0;
            decimal Tkabrab0;
            double Rsecrab = 0;
            double Psecrab = 0;
            double Pkabrab = 0;
            int Tkabrab = Ttr;

            do
            {
                iteration++;
                Rsecrab = Rsec20 * (1 + double.Parse(cable.Alfa) * (Tkabrab - 20));
                Psecrab = (Urab * Urab) / (3 * Rsecrab);

                if (connectionScheme == "петля" || connectionScheme == "две петли" || connectionScheme == "три петли")
                {
                    Psecrab = (Urab * Urab) / Rsecrab;
                }

                Pkabrab = Psecrab / Lsec;
                Tkabrab0 = (decimal)Pkabrab / (60m * 3.14m * (cable.Dkab /1000)) +Ttr;

                if (Math.Abs(Tkabrab0 - Tkabrab) >= 1m)
                {
                    Tkabrab = (int)Tkabrab0;
                }

            } while (Math.Abs(Tkabrab0 - Tkabrab) >= 1m && iteration < 5);

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

