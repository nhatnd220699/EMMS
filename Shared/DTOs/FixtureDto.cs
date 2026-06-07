namespace Shared.DTOs
{
    public class FixtureDto
    {
        public string FixtureId { get; set; }
        public string FixtureName { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
