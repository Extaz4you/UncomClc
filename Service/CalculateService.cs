using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UncomClc.Models;

namespace UncomClc.Service
{
    public class CalculateService
    {
        public void Calculation(GeneralStructure structure)
        {
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


            MessageBox.Show(structure.Name);
        }
    }
}
