namespace MobileBgWatch.Services
{
    public interface IReceiveEmailsService
    {
        public Task<bool> GetReceiveEmailsStatusAsync(string userId);

        public Task ToggleReceiveEmailsAsync(string userId, bool receiveEmails);
    }
}
