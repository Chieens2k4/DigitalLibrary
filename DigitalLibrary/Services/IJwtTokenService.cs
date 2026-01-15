using DigitalLibrary.Models;

namespace DigitalLibrary.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
