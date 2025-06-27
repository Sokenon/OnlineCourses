using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OnlineCourses.Services.Classes
{
    public class AuthOptions
    {
        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key) => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
