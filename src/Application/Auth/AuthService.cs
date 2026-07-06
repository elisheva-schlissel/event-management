using Application.Abstractions;
using Contracts;

namespace Application.Auth;

/// <summary>מטפל בהתחברות: אימות סיסמה והנפקת JWT.</summary>
public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtTokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    /// <summary>מחזיר LoginResponse בהצלחה, או null אם האימות נכשל.</summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(request.Username, ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return null;

        return new LoginResponse
        {
            Token = _tokens.CreateToken(user),
            Role = user.Role.ToString(),
            Name = user.Name
        };
    }
}
