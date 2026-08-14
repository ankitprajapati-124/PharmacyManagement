using Microsoft.AspNetCore.Identity;
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly PasswordHasher<User> _passwordHasher;

    private static readonly string[] AllowedRoles =
    [
        "Admin",
        "Pharmacist",
        "Staff"
    ];

    public UserService(
        IUserRepository repository)
    {
        _repository = repository;

        _passwordHasher =
            new PasswordHasher<User>();
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<int> CreateAsync(
        string username,
        string fullName,
        string password,
        string role)
    {
        username = username.Trim();
        fullName = fullName.Trim();
        role = role.Trim();

        if (!AllowedRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid user role.");
        }

        var existing =
            await _repository.GetByUsernameAsync(
                username);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "Username already exists.");
        }

        var user = new User
        {
            Username = username,
            FullName = fullName,
            Role = role,
            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                password);

        return await _repository.AddAsync(user);
    }

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive)
    {
        return await _repository.SetActiveAsync(
            id,
            isActive);
    }
}