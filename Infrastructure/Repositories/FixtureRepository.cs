using Application.Interfaces;
using Dapper;
using Shared.Common;
using Shared.DTOs;
using Shared.Enums;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Infrastructure.Repositories
{
    public class FixtureRepository : BaseRepository, IFixtureRepository
    {
        private DynamicParameters BuildParameters(
            string funcType,
            string type = "",
            string line = "",
            string department = "",
            string eqSn = "",
            string chkDay = "",
            string extendedPara = "",
            string uploadPathImage = "",
            string uploadResult = "",
            string approvalResult = "",
            string remark = "")
        {
            var parameters = new DynamicParameters();
            parameters.Add("FuncType", funcType ?? "");
            parameters.Add("Type", type.NormalizeFilter());
            parameters.Add("Line", line.NormalizeFilter());
            parameters.Add("EQ_Type", department.NormalizeFilter());
            parameters.Add("EQ_SN", eqSn ?? "");
            parameters.Add("ChkDay", chkDay ?? "");
            parameters.Add("ExtendedPara", extendedPara ?? "");
            parameters.Add("Upload_PathImage", uploadPathImage ?? "");
            parameters.Add("Upload_Result", uploadResult ?? "");
            parameters.Add("Approval_Result", approvalResult ?? "");
            parameters.Add("Remark", remark ?? "");

            return parameters;
        }

        private string EscapeSplitValue(string value)
        {
            return value.ToText()
                .Replace(";", "；")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private string BuildExtendedPara(Dictionary<string, string> values)
        {
            if (values == null) return "";

            return string.Join(";", values
                .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Key + "=" + EscapeSplitValue(x.Value)));
        }

        private async Task<List<string>> GetStringListAsync(string funcType)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters(funcType),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    var rows = await multi.ReadAsync<dynamic>();

                    return rows
                        .Select(x => ((IDictionary<string, object>)x)
                            .Values
                            .FirstOrDefault()?.ToString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                }
            }
        }
        public Task<List<string>> GetTypesAsync()
        {
            return GetStringListAsync(FuncType.Get_Type.ToCode());
        }
        public Task<List<string>> GetLinesAsync()
        {
            return GetStringListAsync(FuncType.Get_Line.ToCode());
        }
        public Task<List<string>> GetDepartmentsAsync()
        {
            return GetStringListAsync(FuncType.Get_Dept.ToCode());
        }

        public async Task<List<FixtureDashboardDto>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn)
        {

            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters(FuncType.Get_Equipment_List.ToCode(), type, line, department, sqSn),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    var rows = (await multi.ReadAsync<FixtureDashboardDto>()).ToList();

                    return rows;
                }
            }

        }

        public async Task<FixtureMonthlyRawDto> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters(FuncType.Get_Equipment_By_Month.ToCode(), type, line, department, eqSn, chkDay),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    var header = (await multi.ReadAsync<FixtureMonthlyHeaderRawDto>()).FirstOrDefault();

                    var rows = (await multi.ReadAsync<FixtureMonthlyRowRawDto>()).ToList();

                    return new FixtureMonthlyRawDto
                    {
                        Header = header,
                        Rows = rows
                    };
                }
            }
        }

        public async Task<FixtureDailyRawDto> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode)
        {
            var extendedPara = BuildExtendedPara(new Dictionary<string, string>
            {
                { "Upload_ID", uploadId }
            });

            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters(FuncType.Get_Equipment_By_Day.ToCode(), type, line, department, eqSn, chkDay, extendedPara),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    var header = (await multi.ReadAsync<FixtureDailyHeaderRawDto>()).FirstOrDefault();

                    var rows = (await multi.ReadAsync<FixtureDailyRowRawDto>()).ToList();

                    return new FixtureDailyRawDto
                    {
                        Header = header,
                        Rows = rows
                    };
                }
            }
        }

        public async Task<string> SaveFixtureDailyAsync(FixtureDailyDto fixture, SaveFixtureParam param)
        {

            var funcType = "";

            if (string.Equals(param.Action, "create", StringComparison.OrdinalIgnoreCase))
            {
                funcType = FuncType.Upload_Equipment_By_SN.ToCode();
            }
            else if (string.Equals(param.Action, "edit", StringComparison.OrdinalIgnoreCase))
            {
                funcType = FuncType.Edit_Equipment_By_SN.ToCode();
            }
            else if (string.Equals(param.Action, "approve", StringComparison.OrdinalIgnoreCase))
            {
                funcType = FuncType.Approve_Equipment_By_SN.ToCode();
            }

            if (string.IsNullOrWhiteSpace(funcType))
            {
                throw new BusinessException("FuncType undefined");
            }

            var parameters = BuildParameters(
               funcType,
               param.Type,
               fixture.Line,
               param.Department,
               fixture.EQ_SN,
               "",
               param.ExtendedPara,
               param.ImageResult,
               param.UploadResult,
               param.ApproveResult,
               param.Remark
           );

            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    return result.Result;
                }
            }
        }
    }
}
