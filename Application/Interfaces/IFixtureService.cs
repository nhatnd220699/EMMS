using Newtonsoft.Json.Linq;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Application.Interfaces
{
    public interface IFixtureService
    {
        Task<Result<FixtureFilterOptionDto>> GetFilterOptionsAsync();
        //Task<ApiResponse<FixtureFilterOptionDto>> GetFilterOptionsAsync();
        Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn);
        Task<ApiResponse<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay);
        Task<ApiResponse<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);
        //Task<ApiResponse<FixtureDailyDto>> SaveFixtureDailyAsync(string type, string line, string department, FixtureDailyDto fixture);
        Task<ApiResponse<string>> SaveFixtureDailyAsync(string type, string line, string department, string funcType, string uploadPathImage, FixtureDailyDto fixture);

        Task<ApiResponse<string>> SaveFixtureDaily2Async(FixtureDaily2Dto model, HttpFileCollection files);

        Task<JObject> GetImageInfoAsync(HttpPostedFile file);
    }
}
