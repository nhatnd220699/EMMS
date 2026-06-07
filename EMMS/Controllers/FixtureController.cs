using Application.Interfaces;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using System.Threading.Tasks;
using System.Web.Http;
namespace EMMS.Controllers
{
    [RoutePrefix("api/fixture")]
    public class FixtureController : ApiController
    {
        private readonly IFixtureService _fixtureService;
        public FixtureController()
        {
            _fixtureService = new FixtureService(new FixtureRepository());
        }

        [HttpGet]
        [Route("dashboard")]
        public async Task<IHttpActionResult> GetFixtureDashboard(string fixtureId, string line, int status)
        {
            var result = await _fixtureService.GetFixtureDashboardsAsync(fixtureId, line, status);
            return Ok(result);
        }
    }
}