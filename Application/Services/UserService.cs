using Application.Interfaces;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //public async Task<ApiResponse<LoginResultDto>> LoginAsync(LoginRequestDto model)
        //{
        //    try
        //    {
        //        if (model == null)
        //        {
        //            return new ApiResponse<LoginResultDto>
        //            {
        //                Success = false,
        //                Message = "Login data is required.",
        //                Data = null
        //            };
        //        }

        //        var type = string.IsNullOrWhiteSpace(model.Type) ? "FATP" : model.Type.Trim();
        //        var userId = model.UserId.ToText().Trim();
        //        var password = model.Password.ToText();

        //        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
        //        {
        //            return new ApiResponse<LoginResultDto>
        //            {
        //                Success = false,
        //                Message = "Please enter User ID and Password.",
        //                Data = null
        //            };
        //        }

        //        var loginResult = await _userRepository.LoginAsync(type, userId, password);
        //        if (!loginResult.Result.IsPass())
        //        {
        //            return new ApiResponse<LoginResultDto>
        //            {
        //                Success = false,
        //                Message = loginResult.Message,
        //                Data = null
        //            };
        //        }

        //        var userResult = await _userRepository.GetUserAsync(type, userId);
        //        if (!userResult.Result.IsPass() || userResult.Data == null)
        //        {
        //            return new ApiResponse<LoginResultDto>
        //            {
        //                Success = false,
        //                Message = string.IsNullOrWhiteSpace(userResult.Message) ? "Get user information failed." : userResult.Message,
        //                Data = null
        //            };
        //        }

        //        var user = userResult.Data;
        //        var currentRole = string.Equals(user.Role.ToText(), "Admin", StringComparison.OrdinalIgnoreCase)
        //            ? "approver"
        //            : "uploader";

        //        return new ApiResponse<LoginResultDto>
        //        {
        //            Success = true,
        //            Message = loginResult.Message,
        //            Data = new LoginResultDto
        //            {
        //                UserID = user.UserID,
        //                UserName = user.UserName,
        //                Email = user.Email,
        //                BU = user.BU,
        //                Type = user.Type,
        //                Customer = user.Customer,
        //                Dept = user.Dept,
        //                RoleId = user.RoleId,
        //                Role = user.Role,
        //                CurrentRole = currentRole
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse<LoginResultDto>
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = null
        //        };
        //    }
        //}

        public async Task<Result<LoginResultDto>> LoginAsync(LoginRequestDto model)
        {
            try
            {
                if (model == null)
                {
                    throw new BusinessException("Login data is required.");
                }

                var type = string.IsNullOrWhiteSpace(model.Type) ? "FATP" : model.Type.Trim();
                var userId = model.UserId.ToText().Trim();
                var password = model.Password.ToText();

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                {
                    throw new BusinessException("Please enter User ID and Password.");
                }

                var loginResult = await _userRepository.LoginAsync(type, userId, password);
                if (!loginResult.Result.IsPass())
                {
                    throw new BusinessException(loginResult.Message);

                }

                var userResult = await _userRepository.GetUserAsync(type, userId);
                if (!userResult.Result.IsPass() || userResult.Data == null)
                {
                    throw new BusinessException(string.IsNullOrWhiteSpace(userResult.Message) ? "Get user information failed." : userResult.Message);

                }

                var user = userResult.Data;
                var currentRole = string.Equals(user.Role.ToText(), "Admin", StringComparison.OrdinalIgnoreCase)
                    ? "approver"
                    : "uploader";

                var data = new LoginResultDto
                {
                    UserID = user.UserID,
                    UserName = user.UserName,
                    Email = user.Email,
                    BU = user.BU,
                    Type = user.Type,
                    Customer = user.Customer,
                    Dept = user.Dept,
                    RoleId = user.RoleId,
                    Role = user.Role,
                    CurrentRole = currentRole
                };

                return Result<LoginResultDto>.Ok(data, loginResult.Message);
            }
            catch (SqlException)
            {
                return Result<LoginResultDto>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<LoginResultDto>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<LoginResultDto>.Fail("System error", "SYSTEM_ERROR");
            }
        }

        public async Task<Result<object>> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {

                if (model == null)
                    throw new BusinessException("Invalid request.");

                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                    throw new BusinessException("Current password is required.");

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                    throw new BusinessException("New password is required.");

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                    throw new BusinessException("Confirm password is required.");

                if (model.NewPassword != model.ConfirmPassword)
                    throw new BusinessException("Passwords do not match.");

                // Ít nhất 8 ký tự
                if (model.NewPassword.Length < 10)
                    throw new BusinessException("Password must be at least 8 characters long.");

                // Chứa chữ hoa
                if (!model.NewPassword.Any(char.IsUpper))
                    throw new BusinessException("Password must contain at least one uppercase letter.");

                // Chứa chữ thường
                if (!model.NewPassword.Any(char.IsLower))
                    throw new BusinessException("Password must contain at least one lowercase letter.");

                // Chứa số
                if (!model.NewPassword.Any(char.IsDigit))
                    throw new BusinessException("Password must contain at least one number.");

                // Chứa ký tự đặc biệt
                if (!model.NewPassword.Any(c => !char.IsLetterOrDigit(c)))
                    throw new BusinessException("Password must contain at least one special character.");


                var repoResult = await _userRepository.ChangePasswordAsync(model);

                return Result<object>.Ok(null, "success");
            }
            catch (SqlException)
            {
                return Result<object>.Fail("Database error", "DB_ERROR");
            }
            catch (BusinessException ex)
            {
                return Result<object>.Fail(ex.Message, "BUSINESS_ERROR");
            }
            catch (Exception)
            {
                return Result<object>.Fail("System error", "SYSTEM_ERROR");
            }
        }
    }
}
