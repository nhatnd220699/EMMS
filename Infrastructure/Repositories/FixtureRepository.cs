using Application.Interfaces;
using Dapper;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Shared.Extensions;

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
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    if (result == null)
                    {
                        throw new BusinessException("Stored procedure returned no status.");
                    }

                    if (!string.Equals(result.Result, "PASS", StringComparison.OrdinalIgnoreCase))
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
            return GetStringListAsync("Get_Type");
        }
        public Task<List<string>> GetLinesAsync()
        {
            return GetStringListAsync("Get_Line");
        }
        public Task<List<string>> GetDepartmentsAsync()
        {
            return GetStringListAsync("Get_Dept");
        }

        public async Task<List<FixtureDashboardDto>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn)
        {

            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters("Get_Equipment_List", type, line, department, sqSn),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    if (result == null)
                    {
                        throw new BusinessException("Stored procedure returned no status.");
                    }

                    if (!string.Equals(result.Result, "PASS", StringComparison.OrdinalIgnoreCase))
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
                    BuildParameters("Get_Equipment_By_Month", type, line, department, eqSn, chkDay),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    if (result == null)
                    {
                        throw new BusinessException("Stored procedure returned no status.");
                    }

                    if (!string.Equals(result.Result, "PASS", StringComparison.OrdinalIgnoreCase))
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
                    BuildParameters("Get_Equipment_By_Day", type, line, department, eqSn, chkDay, extendedPara),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    if (result == null)
                    {
                        throw new BusinessException("Stored procedure returned no status.");
                    }

                    if (!string.Equals(result.Result, "PASS", StringComparison.OrdinalIgnoreCase))
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

        public async Task<string> SaveFixtureDaily2Async(FixtureDaily2Dto fixture, SaveFixtureParam param)
        {

            var parameters = BuildParameters(
                param.FuncType,
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
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    if (result == null)
                    {
                        throw new BusinessException("Stored procedure returned no status.");
                    }

                    if (!string.Equals(result.Result, "PASS", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BusinessException(result.Message);
                    }

                    return result.Result;
                }
            }
        }


        //private DataDto<T> Fail<T>(string message)
        //{
        //    return new DataDto<T>
        //    {
        //        Result = "FAIL",
        //        Message = message,
        //        Data = default(T)
        //    };
        //}

        //private DataDto<T> Fail<T>(ResultMessageDto result)
        //{
        //    return new DataDto<T>
        //    {
        //        Result = result?.ResultValue ?? "FAIL",
        //        Message = result?.MessageValue ?? "SP returned empty result.",
        //        Data = default(T)
        //    };
        //}

        //private DataDto<T> Pass<T>(ResultMessageDto result, T data)
        //{
        //    return new DataDto<T>
        //    {
        //        Result = result?.ResultValue ?? "PASS",
        //        Message = result?.MessageValue ?? "Get data OK!",
        //        Data = data
        //    };
        //}

        //private bool IsPass(string result)
        //{
        //    return string.Equals(result, "PASS", StringComparison.OrdinalIgnoreCase);
        //}

        //private string GetExceptionMessage(Exception ex)
        //{
        //    return ex.GetBaseException()?.Message ?? ex.Message;
        //}

        //private string GetFirstColumnValue(dynamic row)
        //{
        //    var dict = row as IDictionary<string, object>;
        //    if (dict == null || dict.Count == 0) return "";

        //    var value = dict.First().Value;
        //    return value == null ? "" : value.ToString();
        //}

        //private int ToInt(object value)
        //{
        //    if (value == null || value == DBNull.Value) return 0;

        //    int result;
        //    return int.TryParse(value.ToString(), out result) ? result : 0;
        //}

        //private string ToText(object value)
        //{
        //    return value == null || value == DBNull.Value ? "" : value.ToString();
        //}

        //private DateTime ToDate(object value)
        //{
        //    var text = ToText(value);
        //    if (string.IsNullOrWhiteSpace(text)) return DateTime.Today;

        //    DateTime result;
        //    if (DateTime.TryParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //        return result;

        //    if (DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //        return result;

        //    return DateTime.TryParse(text, out result) ? result : DateTime.Today;
        //}

        //private string ToChkDay(DateTime date)
        //{
        //    return date.ToString("yyyyMMdd");
        //}

        //private string NormalizeResultValue(string value)
        //{
        //    value = ToText(value).Trim().ToUpperInvariant();
        //    return value == "OK" || value == "NG" || value == "NA" ? value : "NA";
        //}

        //private string BuildMtValueString(IEnumerable<DailyItemInputDto> items, Func<DailyItemInputDto, string> valueSelector, bool normalizeResult = false)
        //{
        //    if (items == null) return "";

        //    return string.Join(";", items
        //        .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
        //        .Select(x => new
        //        {
        //            x.MT_No,
        //            Value = normalizeResult ? NormalizeResultValue(valueSelector(x)) : EscapeSplitValue(valueSelector(x))
        //        })
        //        .Where(x => !string.IsNullOrWhiteSpace(x.Value))
        //        .Select(x => x.MT_No + "=" + x.Value));
        //}

        //private string EscapeJsonString(string value)
        //{
        //    return ToText(value)
        //        .Replace("\\", "\\\\")
        //        .Replace("\"", "\\\"")
        //        .Replace("\r", "\\r")
        //        .Replace("\n", "\\n");
        //}

        //private string UnescapeJsonString(string value)
        //{
        //    return ToText(value)
        //        .Replace("\\n", "\n")
        //        .Replace("\\r", "\r")
        //        .Replace("\\\"", "\"")
        //        .Replace("\\\\", "\\");
        //}

        //private List<string> ParseImagePaths(string dbImageRaw)
        //{
        //    var result = new List<string>();
        //    if (string.IsNullOrWhiteSpace(dbImageRaw)) return result;

        //    dbImageRaw = dbImageRaw.Trim();

        //    if (dbImageRaw.StartsWith("[") && dbImageRaw.EndsWith("]"))
        //    {
        //        var matches = Regex.Matches(dbImageRaw, "\"((?:\\\\.|[^\"\\\\])*)\"");
        //        foreach (Match match in matches)
        //        {
        //            var value = UnescapeJsonString(match.Groups[1].Value);
        //            if (!string.IsNullOrWhiteSpace(value))
        //                result.Add(value.Trim());
        //        }

        //        return result.Distinct().ToList();
        //    }

        //    return dbImageRaw
        //        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
        //        .Select(x => x.Trim())
        //        .Where(x => !string.IsNullOrWhiteSpace(x))
        //        .Distinct()
        //        .ToList();
        //}

        //private string SerializeImagePaths(List<string> imagePaths)
        //{
        //    imagePaths = (imagePaths ?? new List<string>())
        //        .Where(x => !string.IsNullOrWhiteSpace(x))
        //        .Select(x => x.Trim())
        //        .Distinct()
        //        .ToList();

        //    return "[" + string.Join(",", imagePaths.Select(x => "\"" + EscapeJsonString(x) + "\"")) + "]";
        //}
    }
}
