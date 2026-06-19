using Shared.Common;
using Shared.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<LoginResultDto>> LoginAsync(LoginRequestDto model);
        Task<Result<object>> ChangePasswordAsync(ChangePasswordDto model);
    }
}
