using Domain.Entities;

namespace Application.Abstractions;

/// <summary>מנפיק JWT לאחר התחברות מוצלחת. המימוש ב-Infrastructure.</summary>
public interface IJwtTokenService
{
    string CreateToken(User user);
}
