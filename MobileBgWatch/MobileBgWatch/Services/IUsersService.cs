using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public interface IUsersService
    {
        Task<bool> UserSearchUrlLimitAsync(string userId);

        Task<bool> SearchUrlAlreadyExist(string userId, string searchUrl);
    }
}
