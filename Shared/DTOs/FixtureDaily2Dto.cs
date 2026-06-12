using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Shared.DTOs
{
    public class FixtureDaily2Dto
    {
        public string EQ_SN { get; set; }
        public string EQ_Name { get; set; }
        public string EQ_Model { get; set; }
        public string Line { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        public bool IsApproverMode { get; set; }

        public List<DailyItem2Dto> CheckItems { get; set; } = new List<DailyItem2Dto>();
    }

    public class DailyItem2Dto
    {
        public int Number_Order { get; set; }
        public string MT_No { get; set; }
        public string MT_Item { get; set; }
        public string MT_Desc { get; set; }

        public string Upload_Result { get; set; } // OK, NG, NA
        public string Upload_ID { get; set; }
        public string Approval_ID { get; set; }
        public string Approval_Result { get; set; } // OK, NG, NA
        public string Remark { get; set; }

        public List<string> NewImagePaths { get; set; } = new List<string>();
        public List<string> ExistingImagePaths { get; set; } = new List<string>(); // Để hiển thị ảnh cũ nếu là chế độ Edit
    }
}
