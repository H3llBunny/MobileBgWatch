using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public interface IUsersService
    {
        Task<bool> UserSearchUrlLimitAsync(string userId);
    }
}
