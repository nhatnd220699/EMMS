using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class FixtureMonthlyHeaderRawDto
    {
        public string EQ_SN { get; set; }
        public string EQ_Name { get; set; }
        public string EQ_Model { get; set; }
        public string Line { get; set; }
        public string Shift { get; set; }
        public object Year { get; set; }
        public object Month { get; set; }
    }
}
