using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<int> AddAsync(User user);
}