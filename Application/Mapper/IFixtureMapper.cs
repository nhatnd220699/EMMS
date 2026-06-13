using Shared.DTOs;
using System.Web;

namespace Application.Mapper
{
    public interface IFixtureMapper
    {
        FixtureDaily2Dto Map(HttpRequest request);
    }
}
