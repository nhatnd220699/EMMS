using System.Collections.Generic;

namespace Shared.DTOs
{
    public class FixtureMonthlyDto
    {
        // Header
        public string EQ_SN { get; set; }
        public string EQ_Name { get; set; }
        public string EQ_Model { get; set; }
        public string Line { get; set; }
        public string Shift { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // MT_Item
        public List<MonthlyItemDto> MaintenanceItems { get; set; } = new List<MonthlyItemDto>();
    }

    public class MonthlyItemDto
    {
        public int Number_Order { get; set; }
        public string MT_No { get; set; }
        public string MT_Item { get; set; }
        public string MT_Desc { get; set; }

        // Follow by days of month: { Key: Day (1->31), Value: Status (OK/NG/NA) }
        public Dictionary<int, string> DailyResults { get; set; } = new Dictionary<int, string>();
    }
}
