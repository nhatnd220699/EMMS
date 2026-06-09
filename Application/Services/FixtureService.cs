using Application.Interfaces;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Application.Services
{
    public class FixtureService : IFixtureService
    {
        private readonly IFixtureRepository _fixtureRepository;

        public FixtureService(IFixtureRepository fixtureRepository)
        {
            _fixtureRepository = fixtureRepository;
        }

        private string UnescapeJsonString(string value)
        {
            return value.ToText()
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        private List<string> ParseImagePaths(string dbImageRaw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(dbImageRaw)) return result;

            dbImageRaw = dbImageRaw.Trim();

            if (dbImageRaw.StartsWith("[") && dbImageRaw.EndsWith("]"))
            {
                var matches = Regex.Matches(dbImageRaw, "\"((?:\\\\.|[^\"\\\\])*)\"");
                foreach (Match match in matches)
                {
                    var value = UnescapeJsonString(match.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(value))
                        result.Add(value.Trim());
                }

                return result.Distinct().ToList();
            }

            return dbImageRaw
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public async Task<ApiResponse<FixtureFilterOptionDto>> GetFilterOptionsAsync()
        {
            try
            {
                var typeResult = await _fixtureRepository.GetTypesAsync();
                var lineResult = await _fixtureRepository.GetLinesAsync();
                var departmentResult = await _fixtureRepository.GetDepartmentsAsync();

                var success = typeResult.Result.IsPass() && lineResult.Result.IsPass() && departmentResult.Result.IsPass();

                return new ApiResponse<FixtureFilterOptionDto>
                {
                    Success = success,
                    Message = success ? "Get filter options OK!" : "Get filter options failed.",
                    Data = success ? new FixtureFilterOptionDto
                    {
                        Types = typeResult.Data,
                        Lines = lineResult.Data,
                        Departments = departmentResult.Data
                    } : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureFilterOptionDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDashboardsAsync(type, line, department);

                var success = repoResult.Result.IsPass();

                var rows = repoResult.Data;
                var data = rows
                            .OrderBy(x => x.Number_Order)
                            .ThenBy(x => x.EQ_SN)
                            .ToList();

                return new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? data : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<FixtureDashboardDto>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureMonthlyDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureMonthlyAsync(type, line, department, eqSn);

                var success = repoResult.Result.IsPass();

                var data = repoResult.Data;
                var header = data.Header;
                var rows = data.Rows.ToList();

                var dto = new FixtureMonthlyDto
                {
                    EQ_SN = header.EQ_SN,
                    EQ_Name = header.EQ_Name,
                    EQ_Model = header.EQ_Model,
                    Line = header.Line,
                    Shift = header.Shift,
                    Year = header.Year.ToInt(),
                    Month = header.Month.ToInt(),
                    MaintenanceItems = rows
                        .GroupBy(x => new { MT_No = x.MT_No.ToString(), x.MT_Item, x.MT_Desc })
                        .Select(g => new MonthlyItemDto
                        {
                            Number_Order = g.Min(x => x.Number_Order.ToInt()),
                            MT_No = g.Key.MT_No,
                            MT_Item = g.Key.MT_Item,
                            MT_Desc = g.Key.MT_Desc,
                            DailyResults = g
                                .Where(x => x.Day.ToInt() > 0)
                                .GroupBy(x => x.Day.ToInt())
                                .ToDictionary(x => x.Key, x => x.Last().Status ?? "NA")
                        })
                        .OrderBy(x => x.Number_Order)
                        .ToList()
                };

                return new ApiResponse<FixtureMonthlyDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? dto : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureMonthlyDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureDailyDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode)
        {
            try
            {
                var repoResult = await _fixtureRepository.GetFixtureDailyAsync(type, line, department, eqSn, chkDay, shift, uploadId, isApproverMode);

                var success = repoResult.Result.IsPass();

                var data = repoResult.Data;
                var header = data.Header;
                var rows = data.Rows.ToList();

                var dto = new FixtureDailyDto
                {
                    EQ_SN = header.EQ_SN,
                    EQ_Name = header.EQ_Name,
                    EQ_Model = header.EQ_Model,
                    Line = header.Line,
                    Date = header.Date.ToDate(),
                    Shift = header.Shift,
                    IsApproverMode = isApproverMode,
                    CheckItems = rows
                        .Select(x => new DailyItemInputDto
                        {
                            Number_Order = x.Number_Order.ToInt(),
                            MT_No = x.MT_No.ToText(),
                            MT_Item = x.MT_Item,
                            MT_Desc = x.MT_Desc,

                            ExistingImagePaths = ParseImagePaths(x.Upload_PathImage.ToText()),
                            Upload_Result = x.Upload_Result.ToText(),
                            Upload_ID = x.Upload_ID.ToText(),

                            Approval_ID = x.Approval_ID.ToText(),
                            Approval_Result = x.Approval_Result.ToText(),

                            Remark = x.Remark.ToText()
                        })
                        .OrderBy(x => x.Number_Order)
                        .ThenBy(x => x.MT_No.ToInt())
                        .ToList()
                };

                return new ApiResponse<FixtureDailyDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? dto : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureDailyDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<FixtureDailyDto>> SaveFixtureDailyAsync(string type, string line, string department, FixtureDailyDto fixture)
        {
            try
            {
                var repoResult = await _fixtureRepository.SaveFixtureDailyAsync(type, line, department, fixture);

                var success = repoResult.Result.IsPass();

                return new ApiResponse<FixtureDailyDto>
                {
                    Success = success,
                    Message = repoResult.Message,
                    Data = success ? repoResult.Data : null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FixtureDailyDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
