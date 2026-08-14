using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByUsernameAsync(string username);

    Task<int> AddAsync(User user);

    Task<bool> SetActiveAsync(int id, bool isActive);
}