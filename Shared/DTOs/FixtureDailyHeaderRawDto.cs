using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class FixtureDailyHeaderRawDto
    {
        public string EQ_SN { get; set; }
        public string EQ_Name { get; set; }
        public string EQ_Model { get; set; }
        public string Line { get; set; }
        public object Date { get; set; }
        public string Shift { get; set; }
    }
}
