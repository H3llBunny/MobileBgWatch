namespace MobileBgWatch.Services
{
    public interface ISearchUrlService
    {
        Task<bool> CanRefreshAsync(string userId, string searchUrl);

        Task UpdateLastRefreshAsync(string userId, string searchUrl);
    }
}
