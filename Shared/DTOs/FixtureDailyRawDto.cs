using System.Collections.Generic;


namespace Shared.DTOs
{
    public class FixtureDailyRawDto
    {
        public FixtureDailyHeaderRawDto Header { get; set; }
        public List<FixtureDailyRowRawDto> Rows { get; set; }
    }
}
