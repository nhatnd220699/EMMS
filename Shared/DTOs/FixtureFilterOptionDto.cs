using System.Collections.Generic;

namespace Shared.DTOs
{
    public class FixtureFilterOptionDto
    {
        public List<string> Types { get; set; } = new List<string>();
        public List<string> Lines { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();
    }
}
