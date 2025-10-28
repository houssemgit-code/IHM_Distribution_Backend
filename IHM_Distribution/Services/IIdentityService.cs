namespace IHM_Distribution.Services
{
    public interface IIdentityService
    {
        string IPAddress { get; }

        string GetCurrentUserName();

        string GetCurrentUserEmail();
    }
}
