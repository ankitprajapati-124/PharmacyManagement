using Microsoft.AspNetCore.Identity;
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;

        _passwordHasher =
            new PasswordHasher<User>();
    }

    public async Task<User?> AuthenticateAsync(
        string username,
        string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user =
            await _userRepository.GetByUsernameAsync(
                username.Trim());

        if (user is null || !user.IsActive)
            return null;

        var result =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        if (result ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        return user;
    }
}