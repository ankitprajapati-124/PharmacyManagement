using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync();

    Task<int> CreateAsync(
        string username,
        string fullName,
        string password,
        string role);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive);
}