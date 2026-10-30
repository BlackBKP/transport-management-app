namespace TMS.Interfaces
{
    public interface IDemoAuthenticationService
    {
        string? Authenticate(string userName, string password);
    }
}
