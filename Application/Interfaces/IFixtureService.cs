using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFixtureService
    {
        Task<ApiResponse<FixtureFilterOptionDto>> GetFilterOptionsAsync();
        Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department);
        Task<ApiResponse<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn);
        Task<ApiResponse<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);
        Task<ApiResponse<FixtureDailyDto>> SaveFixtureDailyAsync(string type, string line, string department, FixtureDailyDto fixture);
    }
}
