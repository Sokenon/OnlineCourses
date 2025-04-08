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

namespace OnlineCourses.Services.Classes
{
    public class AuthService : IAuthentificationService
    {
        public ITokenService tokenService;
        public AuthService(ITokenService Token)
        {
            tokenService = Token;
        }
        public string ValidationInfo(LoginDTO info)
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
            byte[] salt = Convert.FromBase64String("NDU2NTQ2NDI0Mzc=");
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
            Dictionary<string, string> claims = tokenService.DescriptionTokenClaims(token);
            bool check = claims["Role"] == "teacher" ? true : false;
            return check;
        }

    }
}
