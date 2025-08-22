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
            float Kiz2 = param.ThermalIsolation2 == null ?  0 : (float)param.ThermalIsolation2.Koef;
            int a = param.PipelinePlacement == "открытый воздух" ? 30 : 10;
            float Kzap = param.Diam >= 100 ? 1.1f : 1.15f;
            int Tpar = param.StreamingTemperature;
            string Tclass = param.TemperatureClass;
            int Tvalue = param.TemperatureClassValue;
            var bd = FindOutDataBase(param);

            var Lzap = Ltr * KLtr - Ltr;
            var Lzadv = Szadv * Izadv;
            var Lfl = Sfl * Ifl;
            var Lop = Sop * Iop;

            TextBlock.Text += $"\r\nПЕРЕМЕННЫЕ\r\n";
            TextBlock.Text += $"\r\nDtr - {Dtr_m}";
            TextBlock.Text += $"\r\nTst - {Tst_m}";
            TextBlock.Text += $"\r\nLtr - {Ltr}";
            TextBlock.Text += $"\r\nKLtr - {KLtr}";
            TextBlock.Text += $"\r\nTiz1 - {Tiz1_m}";
            TextBlock.Text += $"\r\nTiz2 - {Tiz2_m}";
            TextBlock.Text += $"\r\nTokrmin - {Tokrmin}";
            TextBlock.Text += $"\r\nTokrmax - {Tokrmax}";
            TextBlock.Text += $"\r\nTtr - {Ttr}";
            TextBlock.Text += $"\r\nTtechmax - {Ttechmax}";
            TextBlock.Text += $"\r\nTaddmax - {Taddmax}";
            TextBlock.Text += $"\r\nUf - {Uf}";
            TextBlock.Text += $"\r\nUl - {Ul}";
            TextBlock.Text += $"\r\nIabnom - {Iabnom}";
            TextBlock.Text += $"\r\nUrab - {Urab}";
            TextBlock.Text += $"\r\nTvklmin - {Tvklmin}";
            TextBlock.Text += $"\r\nSzhil - {Szhil}";
            TextBlock.Text += $"\r\nLust - {Lust}";
            TextBlock.Text += $"\r\nSzadv - {Szadv}";
            TextBlock.Text += $"\r\nSop - {Sop}";
            TextBlock.Text += $"\r\nSfl - {Sfl}";
            TextBlock.Text += $"\r\nIzadv - {Izadv}";
            TextBlock.Text += $"\r\nIop - {Iop}";
            TextBlock.Text += $"\r\nIfl - {Ifl}";
            TextBlock.Text += $"\r\nMrBd - {MrBd}";
            TextBlock.Text += $"\r\nKtr - {Ktr}";
            TextBlock.Text += $"\r\nKiz - {Kiz}";
            TextBlock.Text += $"\r\nKiz2 - {Kiz2}";
            TextBlock.Text += $"\r\na - {a}";
            TextBlock.Text += $"\r\nKzap - {Kzap}";
            TextBlock.Text += $"\r\nTpar - {Tpar}";
            TextBlock.Text += $"\r\nTclass - {Tclass}";
            TextBlock.Text += $"\r\nLzap - {Lzap}";
            TextBlock.Text += $"\r\nLzadv - {Lzadv}";
            TextBlock.Text += $"\r\nLfl - {Lfl}";
            TextBlock.Text += $"\r\nLop - {Lop}";
            TextBlock.Text += $"\r\nbd - {bd}\r\n";
            TextBlock.Text += $"\r\nTclass/value  - {Tclass} - {Tvalue}";

            TextBlock.Text += $"\r\n1 РАСЧЕТ\r\n";
            TextBlock.Text += $"\r\nLzap - {Lzap}";
            TextBlock.Text += $"\r\nLzadv - {Lzadv}";
            TextBlock.Text += $"\r\nLfl - {Lfl}";
            TextBlock.Text += $"\r\nLop - {Lop}";

            var Lobsh = Ltr + Lzap + Lzadv + Lfl + Lop;

            TextBlock.Text += $"\r\nLobsh - {Lobsh}";

            double rpot = 0;
            if (param.ThermalIsolation2 != null && !string.IsNullOrEmpty(param.ThermalIsolation2.Name))
            {
                rpot = Kzap * (Ttr - Tokrmin) / (
                    Math.Log(Dtr_m / (Dtr_m - 2 * Tst_m)) / (2 * Math.PI * Ktr) +
                    Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Math.PI * Kiz) +
                    Math.Log((Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) / (Dtr_m + 2 * Tiz1_m)) / (2 * Math.PI * Kiz2) +
                    1 / (Math.PI * (Dtr_m + 2 * Tiz1_m + 2 * Tiz2_m) * a)
                );
            }
            else
            {
                rpot = Kzap * (Ttr - Tokrmin) / (
                    Math.Log(Dtr_m / (Dtr_m - 2 * Tst_m)) / (2 * Math.PI * Ktr) +
                    Math.Log((Dtr_m + 2 * Tiz1_m) / Dtr_m) / (2 * Math.PI * Kiz) +
                    1 / (Math.PI * (Dtr_m + 2 * Tiz1_m) * a)
                );
            }

            TextBlock.Text += $"\r\nТеплопотери: {rpot}\r\n";

            var cables = ExcelReader.ReadCableDataFromExcel(bd);

            var findNeededCable = cables.FirstOrDefault();
            findNeededCable.Resistance = findNeededCable.Resistance / 1000.0;
            TextBlock.Text += $"\r\n Элемент из БД ({bd}): Номер строки: {findNeededCable.RowNumber} Марка: {findNeededCable.Mark} Сечение: {findNeededCable.Cross} Сопротивление: {findNeededCable.Resistance} Альфа: {findNeededCable.Alfa} Дельта: {findNeededCable.Delta} Длина: {findNeededCable.Length}\r\n";
            var result = new CalculateResult { Rpot = rpot };
            return result;
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


    }
}

