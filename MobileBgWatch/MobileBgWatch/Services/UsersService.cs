using Microsoft.AspNetCore.Identity;
using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersService(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }

        public async Task<bool> UserSearchUrlLimitAsync(string userId)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            return user.SearchUrls.Count <= 5;
        }
    }
}
