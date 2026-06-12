using Application.Interfaces;
using Shared.Common;
using Shared.DTOs;
using Shared.Extensions;
using System;
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

        public async Task<ApiResponse<LoginResultDto>> LoginAsync(LoginRequestDto model)
        {
            try
            {
                if (model == null)
                {
                    return new ApiResponse<LoginResultDto>
                    {
                        Success = false,
                        Message = "Login data is required.",
                        Data = null
                    };
                }

                var type = string.IsNullOrWhiteSpace(model.Type) ? "FATP" : model.Type.Trim();
                var userId = model.UserId.ToText().Trim();
                var password = model.Password.ToText();

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                {
                    return new ApiResponse<LoginResultDto>
                    {
                        Success = false,
                        Message = "Please enter User ID and Password.",
                        Data = null
                    };
                }

                var loginResult = await _userRepository.LoginAsync(type, userId, password);
                if (!loginResult.Result.IsPass())
                {
                    return new ApiResponse<LoginResultDto>
                    {
                        Success = false,
                        Message = loginResult.Message,
                        Data = null
                    };
                }

                var userResult = await _userRepository.GetUserAsync(type, userId);
                if (!userResult.Result.IsPass() || userResult.Data == null)
                {
                    return new ApiResponse<LoginResultDto>
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(userResult.Message) ? "Get user information failed." : userResult.Message,
                        Data = null
                    };
                }

                var user = userResult.Data;
                var currentRole = string.Equals(user.Role.ToText(), "Admin", StringComparison.OrdinalIgnoreCase)
                    ? "approver"
                    : "uploader";

                return new ApiResponse<LoginResultDto>
                {
                    Success = true,
                    Message = loginResult.Message,
                    Data = new LoginResultDto
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
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResultDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
