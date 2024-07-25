namespace MobileBgWatch.Services
{
    public interface ISearchUrlService
    {
        Task<bool> CanRefreshAsync(string userId, string searchUrl);

        Task UpdateLastRefreshAsync(string userId, string searchUrl);

        Task UpdateLastRefreshByServiceAsync(string userId, string searchUrl);

        Task ResetRefreshCounterAsync(string userId, string searchUrl);

        Task UpdateRefreshCounterAsync(string userId, string searchUrl);

        Task<bool> DoesUrlExistAsync(string userId, string searchUrl);
    }
}
