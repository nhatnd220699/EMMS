using Shared.DTOs;
using System.Web;

namespace Application.Mapper
{
    public interface IFixtureMapper
    {
        FixtureDailyDto Map(HttpRequest request);
    }
}
