using System.Collections.Generic;

namespace Shared.DTOs
{
    public class FixtureMonthlyRawDto
    {
        public FixtureMonthlyHeaderRawDto Header { get; set; }
        public List<FixtureMonthlyRowRawDto> Rows { get; set; }
    }
}
