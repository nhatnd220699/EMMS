using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFixtureRepository
    {
        Task<List<string>> GetTypesAsync();

        Task<List<string>> GetLinesAsync();

        Task<List<string>> GetDepartmentsAsync();

        Task<List<FixtureDashboardDto>> GetFixtureDashboardsAsync(string userId, string type, string line, string department, string sqSn);

        Task<FixtureMonthlyRawDto> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay);

        Task<FixtureDailyRawDto> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);

        Task<string> SaveFixtureDailyAsync(FixtureDailyDto fixture, SaveFixtureParam param);
    }
}
