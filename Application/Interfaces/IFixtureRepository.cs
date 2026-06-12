using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFixtureRepository
    {
        Task<List<string>> GetTypesAsync();
        Task<List<string>> GetLinesAsync();
        Task<List<string>> GetDepartmentsAsync();
        //Task<DataDto<List<string>>> GetTypesAsync();
        //Task<DataDto<List<string>>> GetLinesAsync();
        //Task<DataDto<List<string>>> GetDepartmentsAsync();
        //Task<DataDto<FixtureFilterOptionDto>> GetFilterOptionsAsync();
        Task<DataDto<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn);
        //Task<DataDto<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn);
        Task<DataDto<FixtureMonthlyRawDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay);
        //Task<DataDto<FixtureDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);
        Task<DataDto<FixtureDailyRawDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode);
        //Task<DataDto<FixtureDailyDto>> SaveFixtureDailyAsync(string type, string line, string department, FixtureDailyDto fixture);
        Task<DataDto<string>> SaveFixtureDailyAsync(string type, string line, string department, string funcType, string uploadPathImage, FixtureDailyDto fixture);
    }
}
