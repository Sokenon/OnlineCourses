using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineCourses.Servers.Interfaces
{
    public interface ITokenService
    {
        public JwtSecurityToken Token(string id, string role);
        public Dictionary<string, string> DescriptionTokenClaims(string token);
        public ClaimsPrincipal Validation(string token, out SecurityToken validatedToken);
        public string GetRole(string token);
        public string GetID(string token);
    }
}
