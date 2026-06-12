using Shared.Common;
using Shared.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<LoginResultDto>> LoginAsync(LoginRequestDto model);
    }
}
