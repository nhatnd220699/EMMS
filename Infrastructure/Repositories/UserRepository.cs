using Application.Interfaces;
using Dapper;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private DynamicParameters BuildParameters(
            string funcType,
            string type = "",
            string userId = "",
            string password = "",
            string newPassword = "")
        {
            var parameters = new DynamicParameters();
            parameters.Add("FuncType", funcType ?? "");
            parameters.Add("Type", string.IsNullOrWhiteSpace(type) ? "FATP" : type.Trim());
            parameters.Add("Line", "");
            parameters.Add("EQ_Type", "");
            parameters.Add("EQ_SN", "");
            parameters.Add("ChkDay", "");
            parameters.Add("ExtendedPara", BuildExtendedPara(new Dictionary<string, string>
            {
                { "UserId", userId },
                { "Password", password },
                { "NewPassword", newPassword }
            }));
            parameters.Add("Upload_PathImage", "");
            parameters.Add("Upload_Result", "");
            parameters.Add("Approval_Result", "");
            parameters.Add("Remark", "");

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

        private string GetExceptionMessage(Exception ex)
        {
            return ex.GetBaseException()?.Message ?? ex.Message;
        }

        private string ToText(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString();
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
                .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Value != null)
                .Select(x => x.Key + "=" + EscapeSplitValue(x.Value)));
        }

        //public async Task<DataDto<bool>> LoginAsync(string type, string userId, string password)
        //{
        //    try
        //    {
        //        using (var conn = _db.CreateConnection())
        //        {
        //            using (var multi = await conn.QueryMultipleAsync(
        //                "Maintenance_Fixture_Portal",
        //                BuildParameters("Login", type, userId, password),
        //                commandType: CommandType.StoredProcedure,
        //                commandTimeout: 180)
        //            )
        //            {
        //                var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();
        //                if (result == null || !result.IsPass)
        //                    return Fail<bool>(result);

        //                return new DataDto<bool>
        //                {
        //                    Result = result.Result,
        //                    Message = result.Message,
        //                    Data = true
        //                };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail<bool>("Call SP Login error: " + GetExceptionMessage(ex));
        //    }
        //}

        //public async Task<DataDto<UserDto>> GetUserAsync(string type, string userId)
        //{
        //    try
        //    {
        //        using (var conn = _db.CreateConnection())
        //        {
        //            using (var multi = await conn.QueryMultipleAsync(
        //                "Maintenance_Fixture_Portal",
        //                BuildParameters("Get_User", type, userId),
        //                commandType: CommandType.StoredProcedure,
        //                commandTimeout: 180)
        //            )
        //            {
        //                var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault();
        //                if (result == null || !result.IsPass)
        //                    return Fail<UserDto>(result);

        //                var user = (await multi.ReadAsync<UserDto>()).FirstOrDefault();
        //                if (user == null)
        //                    return Fail<UserDto>("User information not found.");

        //                return new DataDto<UserDto>
        //                {
        //                    Result = result.Result,
        //                    Message = result.Message,
        //                    Data = user
        //                };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail<UserDto>("Call SP Get_User error: " + GetExceptionMessage(ex));
        //    }
        //}

        public async Task<DataDto<bool>> LoginAsync(string type, string userId, string password)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters("Login", type, userId, password),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                        throw new BusinessException(result.Message);

                    return new DataDto<bool>
                    {
                        Result = result.Result,
                        Message = result.Message,
                        Data = true
                    };
                }
            }
        }

        public async Task<DataDto<UserDto>> GetUserAsync(string type, string userId)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters("Get_User", type, userId),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    var user = (await multi.ReadAsync<UserDto>()).FirstOrDefault();

                    if (user == null)
                    {
                        throw new BusinessException("User information not found.");
                    }

                    return new DataDto<UserDto>
                    {
                        Result = result.Result,
                        Message = result.Message,
                        Data = user
                    };
                }
            }
        }

        public async Task<object> ChangePasswordAsync(ChangePasswordDto model)
        {

            using (var conn = _db.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(
                    "Maintenance_Fixture_Portal",
                    BuildParameters("Change_Password", model.Type, model.UserId, model.CurrentPassword, model.NewPassword),
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180)
                )
                {
                    var result = (await multi.ReadAsync<ResultMessageDto>()).FirstOrDefault() ?? throw new BusinessException("Stored procedure returned no status.");

                    if (!result.Result.IsPass())
                    {
                        throw new BusinessException(result.Message);
                    }

                    return new
                    {
                        result.Result,
                        result.Message
                    };
                }
            }
        }
    }
}
