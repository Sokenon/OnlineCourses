using OnlineCourses.Models.DTO;
using OnlineCourses.Models;


namespace OnlineCourses.Services.Interfaces
{
    public interface IUserService
    {
        public User? FindUser(string email, string passwordHash);
        public User? FindUser(int id);
        public bool CheckUser(string email);
        public User AddNewUser(string email, string passwordHash);
        public void SetName(string username, int id);

    }
}
