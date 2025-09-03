using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomClc.Models.Cable
{
    public class CableModel
    {
        public int RowNumber { get; set; }
        public string Mark { get; set; }
        public string Cross {  get; set; }
        public double Resistance {  get; set; }
        public string Alfa {  get; set; }
        public string Delta { get; set; }
        public string Length { get; set; }
        public decimal Dkab { get; set; }
    }
}
