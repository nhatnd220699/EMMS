using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class FixtureMonthlyRowRawDto
    {
        public object Number_Order { get; set; }
        public object Day { get; set; }
        public object MT_No { get; set; }
        public string MT_Item { get; set; }
        public string MT_Desc { get; set; }
        public string Status { get; set; }
    }
}
