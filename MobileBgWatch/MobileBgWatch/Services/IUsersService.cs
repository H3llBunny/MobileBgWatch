using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public interface IUsersService
    {
        Task<bool> UserSearchUrlLimitAsync(string userId);

        Task<bool> SearchUrlAlreadyExist(string userId, string searchUrl);

        Task AddSearchUrlToUserAsync(string userId, string searchUrl);

        Task DeleteSearchUrlAsync(string userId, string searchUrl);

        Task<bool> NewPasswordCheckAsync(string userId, string newPassword);

        Task UpdatePasswordAsync(string userId, string newPassword);
    }
}
