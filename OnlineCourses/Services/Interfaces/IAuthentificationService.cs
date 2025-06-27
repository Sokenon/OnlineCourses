using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Servers.Interfaces;

namespace OnlineCourses.Services.Interfaces
{
    public interface IAuthentificationService
    {
        public string ValidationInfoAndHashPassword(LoginDTO info);
        public string Hash(string password);
        public bool CheckTeacher(string token);        
    }
}
