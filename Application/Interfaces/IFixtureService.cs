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

        Task<Result<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string userId, string type, string line, string department, string sqSn, string eqName);

        Task<Result<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay);

        Task<Result<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);

        Task<Result<string>> SaveFixtureDailyAsync(FixtureDailyDto model, HttpFileCollection files, string action, string type, string department);

        Task<Result<JObject>> GetImageInfoAsync(HttpPostedFile file);
    }
}
