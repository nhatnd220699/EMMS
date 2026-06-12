using Shared.Common;
using Shared.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<DataDto<bool>> LoginAsync(string type, string userId, string password);
        Task<DataDto<UserDto>> GetUserAsync(string type, string userId);
    }
}
