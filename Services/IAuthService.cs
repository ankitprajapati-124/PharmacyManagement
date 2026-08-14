using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(
        string username,
        string password);
}