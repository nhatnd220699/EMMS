using Newtonsoft.Json.Linq;
using Shared.Common;
using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Application.Interfaces
{
    public interface IFixtureService
    {
        Task<Result<FixtureFilterOptionDto>> GetFilterOptionsAsync();

        Task<Result<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn);

        Task<Result<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay);

        Task<Result<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);

        Task<Result<string>> SaveFixtureDaily2Async(FixtureDaily2Dto model, HttpFileCollection files, string funcType, string type, string department);

        Task<JObject> GetImageInfoAsync(HttpPostedFile file);
    }
}
