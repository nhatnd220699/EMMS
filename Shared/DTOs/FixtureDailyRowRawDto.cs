using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class FixtureDailyRowRawDto
    {
        public object Number_Order { get; set; }
        public object MT_No { get; set; }
        public string MT_Item { get; set; }
        public string MT_Desc { get; set; }
        public string Upload_PathImage { get; set; }
        public string Upload_Result { get; set; }
        public string Upload_ID { get; set; }
        public string Approval_ID { get; set; }
        public string Approval_Result { get; set; }
        public string Remark { get; set; }
    }
}
