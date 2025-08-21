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
        public void Calculation(GeneralStructure structure)
        {
            if (structure == null) return;
            TextBlock.Text = "";
            var param = structure.Parameters;

            int Dtr = param.Diam;
            int Tst = param.Thickness;
            int Ltr = param.Lenght;
            float KLtr = param.PipeKoef;
            int Tiz1 = param.IsolationThickness;
            int Tiz2 = param.IsolationThickness2;
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
            var bd = FindOutDataBase(param);

            var Lzap = Ltr * KLtr - Ltr;
            var Lzadv = Szadv * Izadv;
            var Lfl = Sfl * Ifl;
            var Lop = Sop * Iop;

            TextBlock.Text += $"\r\nLzap - {Lzap}";
            TextBlock.Text += $"\r\nLzadv - {Lzadv}";
            TextBlock.Text += $"\r\nLfl - {Lfl}";
            TextBlock.Text += $"\r\nKzap - {Kzap}";
            TextBlock.Text += $"\r\nTokrmin - {Tokrmin}";
            TextBlock.Text += $"\r\nDtr - {Dtr}";
            TextBlock.Text += $"\r\nTst - {Tst}";
            TextBlock.Text += $"\r\nKtr - {Ktr}";
            TextBlock.Text += $"\r\nTiz1 - {Tiz1}";
            TextBlock.Text += $"\r\nKiz - {Kiz}";
            TextBlock.Text += $"\r\na - {a}";


            var Lobsh = Ltr + Lzap + Lzadv + Lfl + Lop;

            double rpot = 0;
            if (Tiz2 > 0)
            {
                rpot = Kzap * (Ttr - Tokrmin) / (Math.Log(Dtr / (Dtr - 2 * Tst)) / (2 * Math.PI * Ktr) + Math.Log((Dtr + 2 * Tiz1) / Dtr) / (2 * Math.PI * Kiz) + Math.Log((Dtr + 2 * Tiz1 + 2 * Tiz2) / (Dtr + 2 * Tiz1)) / (2 * Math.PI * Tiz2) + 1 / (Math.PI * (Dtr + 2 * Tiz1 + 2 * Tiz2) * a));
            }
            else
            {
                rpot = Kzap * (Ttr - Tokrmin) / (Math.Log(Dtr / (Dtr - 2 * Tst)) / (2 * Math.PI * Ktr) + Math.Log((Dtr + 2 * Tiz1) / Dtr) / (2 * Math.PI * Kiz) + 1 / (Math.PI * (Dtr + 2 * Tiz1) * a));
            }

            TextBlock.Text += $"\r\nТеплопотери: {rpot}\r\n";

            var cables = ExcelReader.ReadCableDataFromExcel(bd);

            var findNeededCable = cables.FirstOrDefault();
            TextBlock.Text += $"\r\n Элемент из БД ({bd}): Номер строки: {findNeededCable.RowNumber} Марка: {findNeededCable.Mark} Сечение: {findNeededCable.Cross} Сопротивление: {findNeededCable.Resistance} Альфа: {findNeededCable.Alfa} Дельта: {findNeededCable.Delta} Длина: {findNeededCable.Length}\r\n";

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
                if (param.CableType == "КНММ" && param.WorkLoad < 300) return "2КНММ-В3";
                if (param.CableType == "КНМММ" && param.WorkLoad < 300) return "2КНММН-В3";
                if (param.CableType == "КНМС" && param.WorkLoad < 300) return "2КНМС-В3";
                if (param.CableType == "КНМСин" && param.WorkLoad < 300) return "2КНМСин-В3";
                if (param.CableType == "КНМС825" && param.WorkLoad < 300) return "2КНМС825-В3";

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

