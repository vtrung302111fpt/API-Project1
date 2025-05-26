using API_Project1.Services;

namespace API_Project1.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
        Task<string> GetFullLoginResponseAsync();
    }
}
