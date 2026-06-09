using System;
using System.Collections.Generic;

namespace Shared.DTOs
{
    public class FixtureDailyDto
    {
        // Header
        public string EQ_SN { get; set; }
        public string EQ_Name { get; set; }
        public string EQ_Model { get; set; }
        public string Line { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        // Cờ xác định user đang mở form với quyền nào để ẩn/hiện cột tương ứng
        public bool IsApproverMode { get; set; }
        // Input form
        public List<DailyItemInputDto> CheckItems { get; set; } = new List<DailyItemInputDto>();
    }

    public class DailyItemInputDto
    {
        public int Number_Order { get; set; }
        public string MT_No { get; set; }
        public string MT_Item { get; set; }
        public string MT_Desc { get; set; }

        // Input data
        // public string Upload_PathImage { get; set; }
        public string Upload_Result { get; set; } // OK, NG, NA
        public string Upload_ID { get; set; }
        public string Approval_ID { get; set; }
        public string Approval_Result { get; set; } // OK, NG, NA
        public string Remark { get; set; }

        // Project hiện tại dùng ASP.NET MVC/WebAPI (.NET Framework), không dùng ASP.NET Core IFormFile.
        // File ảnh được Controller đọc trực tiếp từ multipart/form-data bằng HttpContext.Current.Request.Files.
        public List<string> ExistingImagePaths { get; set; } = new List<string>(); // Để hiển thị ảnh cũ nếu là chế độ Edit
    }
}
