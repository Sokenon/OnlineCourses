using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Servers.Classes;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineCourses.Services.Classes
{
    public class AuthService : IAuthentificationService
    {
        static public string cookie = "Token";
        private ITokenService tokenService;
        private IConfiguration configuration;

        public AuthService(ITokenService Token, IConfiguration configuration)
        {
            tokenService = Token;
            this.configuration = configuration;
        }
        public string ValidationInfoAndHashPassword(LoginDTO info)
        {
            Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (regex.IsMatch(info.Email))
            {
                return Hash(info.Password);
            }
            else
            {
                Exception error = new Exception("Не корректный e-mail");
                throw error;
            }
        }
        public string Hash (string password)
        {
            byte[] salt = Convert.FromBase64String(configuration.GetSection("Auth").GetValue<string>("Salt")!);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 50,
                numBytesRequested: 256 / 8));
            return hashed;
        }
        public bool CheckTeacher(string token)
        {
            string role = tokenService.GetClaim(token, ClaimTypes.Role);
            bool check = role == "teacher" ? true : false;
            return check;
        }

    }
}
