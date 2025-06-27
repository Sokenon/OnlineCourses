using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Models.Types;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Classes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace OnlineCourses.Servers.Classes
{
    public class JWT : ITokenService
    {
        private IConfiguration configuration;
        public JWT(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public JwtSecurityToken Token(string id, string role)
        {
            var claims = new List<Claim> { new Claim(ClaimsTypes.ID, id), new Claim(ClaimsTypes.Role, role) };
            var jwt = new JwtSecurityToken(
                    issuer: configuration.GetSection("AuthOptions").GetValue<string>("ISSUER"),
                    audience: configuration.GetSection("AuthOptions").GetValue<string>("AUDIENCE"),
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(60)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration.GetSection("AuthOptions").GetValue<string>("KEY")!), SecurityAlgorithms.HmacSha256));

            return jwt;
        }
        public Dictionary<string, string> DescriptionTokenClaims(string token)
        {
            var jwt = new JwtSecurityToken(token);
            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);
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
        public string GetClaim(string token, string claim)
        {
            Dictionary<string, string> claims = DescriptionTokenClaims(token);
            if (claims.ContainsKey(claim))
            {
                return claims[claim];
            }
            else
            {
                throw new Exception($"Claim {claim} not found");
            }
        }
    }
}
