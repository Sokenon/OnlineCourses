using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Servers.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace OnlineCourses.Servers.Classes
{
    public class JWT : ITokenService
    {
        public JwtSecurityToken Token(string id, string role)
        {
            var claims = new List<Claim> { new Claim("ID", id), new Claim("Role", role) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(60)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return jwt;
        }
        public Dictionary<string, string> DescriptionTokenClaims(string token)
        {
            var jwt = new JwtSecurityToken(token);
            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value); //берём клеймсы и расшифровывем
            return claims;
        }
        public ClaimsPrincipal Validation(string token, out SecurityToken validatedToken)
        {
            var parametrs = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            };
            JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
            var principial = _tokenHandler.ValidateToken(token, parametrs, out validatedToken);
            return principial;
        }
        public string GetRole(string token)
        {
            Dictionary<string, string> claims = DescriptionTokenClaims(token);
            string role = claims["Role"];
            return role;
        }
        public string GetID(string token)
        {
            Dictionary<string, string> claims = DescriptionTokenClaims(token);
            string id = claims["ID"];
            return id;
        }

    }
}
