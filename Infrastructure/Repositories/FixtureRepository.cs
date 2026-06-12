using Application.Interfaces;
using Dapper;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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

        private DataDto<T> Fail<T>(string message)
        {
            return new DataDto<T>
            {
                Result = "FAIL",
                Message = message,
                Data = default(T)
            };
        }

        private DataDto<T> Fail<T>(ResultMessageDto result)
        {
            return new DataDto<T>
            {
                Result = result?.ResultValue ?? "FAIL",
                Message = result?.MessageValue ?? "SP returned empty result.",
                Data = default(T)
            };
        }

        private DataDto<T> Pass<T>(ResultMessageDto result, T data)
        {
            return new DataDto<T>
            {
                Result = result?.ResultValue ?? "PASS",
                Message = result?.MessageValue ?? "Get data OK!",
                Data = data
            };
        }

        private bool IsPass(string result)
        {
            return string.Equals(result, "PASS", StringComparison.OrdinalIgnoreCase);
        }

        private string GetExceptionMessage(Exception ex)
        {
            return ex.GetBaseException()?.Message ?? ex.Message;
        }

        private string GetFirstColumnValue(dynamic row)
        {
            var dict = row as IDictionary<string, object>;
            if (dict == null || dict.Count == 0) return "";

            var value = dict.First().Value;
            return value == null ? "" : value.ToString();
        }

        private int ToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;

            int result;
            return int.TryParse(value.ToString(), out result) ? result : 0;
        }

        private string ToText(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString();
        }

        private DateTime ToDate(object value)
        {
            var text = ToText(value);
            if (string.IsNullOrWhiteSpace(text)) return DateTime.Today;

            DateTime result;
            if (DateTime.TryParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            return DateTime.TryParse(text, out result) ? result : DateTime.Today;
        }

        private string ToChkDay(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        private string NormalizeResultValue(string value)
        {
            value = ToText(value).Trim().ToUpperInvariant();
            return value == "OK" || value == "NG" || value == "NA" ? value : "NA";
        }

        private string EscapeSplitValue(string value)
        {
            return ToText(value)
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

        private string BuildMtValueString(IEnumerable<DailyItemInputDto> items, Func<DailyItemInputDto, string> valueSelector, bool normalizeResult = false)
        {
            if (items == null) return "";

            return string.Join(";", items
                .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
                .Select(x => new
                {
                    x.MT_No,
                    Value = normalizeResult ? NormalizeResultValue(valueSelector(x)) : EscapeSplitValue(valueSelector(x))
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.MT_No + "=" + x.Value));
        }

        private string EscapeJsonString(string value)
        {
            return ToText(value)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private string UnescapeJsonString(string value)
        {
            return ToText(value)
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

        private string SerializeImagePaths(List<string> imagePaths)
        {
            imagePaths = (imagePaths ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            return "[" + string.Join(",", imagePaths.Select(x => "\"" + EscapeJsonString(x) + "\"")) + "]";
        }

        private async Task<DataDto<List<string>>> GetStringListAsync(string funcType)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters(funcType),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                    var rows = await multi.ReadAsync<dynamic>();
                    var data = rows
                                .Select(x => ((IDictionary<string, object>)x)
                                    .Values
                                    .FirstOrDefault()?.ToString())
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();


                    return new DataDto<List<string>>
                    {
                        Result = result?.Result,
                        Message = result?.Message,
                        Data = data
                    };
                }
            }
        }

        public Task<DataDto<List<string>>> GetTypesAsync()
        {
            return GetStringListAsync("Get_Type");
        }

        public Task<DataDto<List<string>>> GetLinesAsync()
        {
            return GetStringListAsync("Get_Line");
        }

        public Task<DataDto<List<string>>> GetDepartmentsAsync()
        {
            return GetStringListAsync("Get_Dept");
        }

        public async Task<DataDto<List<FixtureDashboardDto>>> GetFixtureDashboardsAsync(string type, string line, string department, string sqSn)
        {
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    using (var multi = await conn.QueryMultipleAsync(
                        "Maintenance_Fixture_Portal",
                        BuildParameters("Get_Equipment_List", type, line, department, sqSn),
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 180)
                    )
                    {
                        var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                        var data = (await multi.ReadAsync<FixtureDashboardDto>()).ToList();

                        return new DataDto<List<FixtureDashboardDto>>
                        {
                            Result = result?.Result,
                            Message = result?.Message,
                            Data = data
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail<List<FixtureDashboardDto>>("Call SP Get_Equipment_List error: " + GetExceptionMessage(ex));
            }
        }

        public async Task<DataDto<FixtureMonthlyRawDto>> GetFixtureMonthlyAsync(string type, string line, string department, string eqSn, string chkDay)
        {
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    using (var multi = await conn.QueryMultipleAsync(
                        "Maintenance_Fixture_Portal",
                        BuildParameters("Get_Equipment_By_Month", type, line, department, eqSn, chkDay),
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 180)
                    )
                    {
                        var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                        var headers = (await multi.ReadAsync<FixtureMonthlyHeaderRawDto>()).FirstOrDefault();

                        var rows = (await multi.ReadAsync<FixtureMonthlyRowRawDto>()).ToList();

                        return new DataDto<FixtureMonthlyRawDto>
                        {
                            Result = result?.Result,
                            Message = result?.Message,
                            Data = new FixtureMonthlyRawDto
                            {
                                Header = headers,
                                Rows = rows
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail<FixtureMonthlyRawDto>("Call SP Get_Equipment_By_Month error: " + GetExceptionMessage(ex));
            }
        }

        public async Task<DataDto<FixtureDailyRawDto>> GetFixtureDailyAsync(string type, string line, string department, string eqSn, string chkDay, string shift, string uploadId, bool isApproverMode)
        {
            try
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
                        commandTimeout: 180)
                    )
                    {
                        var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                        var header = (await multi.ReadAsync<FixtureDailyHeaderRawDto>()).FirstOrDefault();

                        var rows = (await multi.ReadAsync<FixtureDailyRowRawDto>()).ToList();

                        return new DataDto<FixtureDailyRawDto>
                        {
                            Result = result?.Result,
                            Message = result?.Message,
                            Data = new FixtureDailyRawDto
                            {
                                Header = header,
                                Rows = rows
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail<FixtureDailyRawDto>("Call SP Get_Equipment_By_Day error: " + GetExceptionMessage(ex));
            }
        }


        public async Task<DataDto<string>> SaveFixtureDailyAsync(string type, string line, string department, string funcType, string uploadPathImage, FixtureDailyDto fixture)
        {
            try
            {
                if (fixture == null)
                    return Fail<string>("Fixture data is required.");

                if (string.IsNullOrWhiteSpace(fixture.EQ_SN))
                    return Fail<string>("EQ_SN is required.");

                var items = fixture.CheckItems ?? new List<DailyItemInputDto>();
                if (!items.Any())
                    return Fail<string>("Check item data is required.");

                //var funcType = fixture.IsApproverMode ? "Approve_Equipment_By_SN" : "Upload_Equipment_By_SN";
                var shift = string.IsNullOrWhiteSpace(fixture.Shift) ? "Day" : fixture.Shift;
                var uploadId = items.Select(x => x.Upload_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var approvalId = items.Select(x => x.Approval_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                var extendedPara = fixture.IsApproverMode
                    ? ""
                    : BuildExtendedPara(new Dictionary<string, string>
                    {
                        { "Upload_ID", uploadId },
                        { "Approval_ID", approvalId },
                        { "EQ_Name", fixture.EQ_Name },
                        { "EQ_Model", fixture.EQ_Model }
                    });

                var uploadResult = fixture.IsApproverMode ? "" : BuildMtValueString(items, x => x.Upload_Result, true);
                var approvalResult = fixture.IsApproverMode ? BuildMtValueString(items, x => x.Approval_Result, true) : "";
                var remark = BuildMtValueString(items, x => x.Remark);
                //var uploadPathImage = fixture.IsApproverMode
                //    ? ""
                //    : string.Join(";", items
                //        .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
                //        .Select(x => x.MT_No + "=" + SerializeImagePaths(x.ExistingImagePaths)));

                using (var conn = _db.CreateConnection())
                {
                    using (var multi = await conn.QueryMultipleAsync(
                        "Maintenance_Fixture_Portal",
                        BuildParameters(funcType, type, line, department, fixture.EQ_SN, "", extendedPara, uploadPathImage, uploadResult, approvalResult, remark),
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 180)
                    )
                    {
                        var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

                        // The first table returned by SP is always Result. Only read next tables when Result = PASS.
                        if (result == null || !result.IsPass)
                        {
                            return Fail<string>(result);
                        }
                        else
                        {
                            return new DataDto<string>
                            {
                                Result = result.Result,
                                Message = result.Message,
                                Data = ""
                            };
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                return Fail<string>("Call SP save daily equipment error: " + GetExceptionMessage(ex));
            }
        }

        //public async Task<DataDto<FixtureDailyDto>> SaveFixtureDailyAsync(string type, string line, string department, string imageResult, string uploadResult, string approvalResult, FixtureDailyDto fixture)
        //{
        //    try
        //    {
        //        if (fixture == null)
        //            return Fail<FixtureDailyDto>("Fixture data is required.");

        //        if (string.IsNullOrWhiteSpace(fixture.EQ_SN))
        //            return Fail<FixtureDailyDto>("EQ_SN is required.");

        //        var items = fixture.CheckItems ?? new List<DailyItemInputDto>();
        //        if (!items.Any())
        //            return Fail<FixtureDailyDto>("Check item data is required.");

        //        var funcType = fixture.IsApproverMode ? "Approve_Equipment_By_SN" : "Upload_Equipment_By_SN";
        //        var shift = string.IsNullOrWhiteSpace(fixture.Shift) ? "Day" : fixture.Shift;
        //        var uploadId = items.Select(x => x.Upload_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        //        var approvalId = items.Select(x => x.Approval_ID).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        //        var extendedPara = fixture.IsApproverMode
        //            ? ""
        //            : BuildExtendedPara(new Dictionary<string, string>
        //            {
        //                { "Upload_ID", uploadId },
        //                { "Approval_ID", approvalId },
        //                { "EQ_Name", fixture.EQ_Name },
        //                { "EQ_Model", fixture.EQ_Model }
        //            });

        //        var uploadResult = fixture.IsApproverMode ? "" : BuildMtValueString(items, x => x.Upload_Result, true);
        //        var approvalResult = fixture.IsApproverMode ? BuildMtValueString(items, x => x.Approval_Result, true) : "";
        //        var remark = BuildMtValueString(items, x => x.Remark);
        //        var uploadPathImage = fixture.IsApproverMode
        //            ? ""
        //            : string.Join(";", items
        //                .Where(x => !string.IsNullOrWhiteSpace(x.MT_No))
        //                .Select(x => x.MT_No + "=" + SerializeImagePaths(x.ExistingImagePaths)));

        //        using (var conn = _db.CreateConnection())
        //        {
        //            using (var multi = await conn.QueryMultipleAsync(
        //                "Maintenance_Fixture_Portal",
        //                BuildParameters(funcType, type, line, department, fixture.EQ_SN, "", extendedPara, uploadPathImage, uploadResult, approvalResult, remark),
        //                commandType: CommandType.StoredProcedure,
        //                commandTimeout: 180)
        //            )
        //            {
        //                var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();

        //                // The first table returned by SP is always Result. Only read next tables when Result = PASS.
        //                if (result == null || !result.IsPass)
        //                    return Fail<FixtureDailyDto>(result);
        //            }
        //        }

        //        return await GetFixtureDailyAsync(type, line, department, fixture.EQ_SN, ToChkDay(fixture.Date), shift, uploadId, fixture.IsApproverMode);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail<FixtureDailyDto>("Call SP save daily equipment error: " + GetExceptionMessage(ex));
        //    }
        //}
    }
}
