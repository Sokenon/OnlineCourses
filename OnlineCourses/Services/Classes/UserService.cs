using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Interfaces;


namespace OnlineCourses.Services.Classes

{
    public class UserService : IUserService
    {
        public ApplicationContext db;
        public UserService(ApplicationContext DB)
        {
            db = DB;
        }
        public bool CheckUser(string email)
        {
            User? user = db.Users.FirstOrDefault(u => u.Email == email);
            return user != null;
        }
        public User AddNewStudent(string email, string passwordHash)
        {
            User user = new User { Email = email, PasswordHash = passwordHash, Role = "student" };
            db.Users.AddAsync(user);
            db.SaveChanges();
            return user;
        }

        public User? FindUser(string email, string passwordHash)
        {
            User? user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == passwordHash);
            if (user == null)
            {
                Exception ex = new Exception("Пользователь не найден");
                throw ex;
            }
            else
            {
                return user;
            }
        }
        public User? FindUser(int id)
        {
            User? user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                Exception ex = new Exception("Пользователь не найден");
                throw ex;
            }
            else
            {
                return user;
            }
        }
        public void SetName (string newUsername, int id)
        {
            db.Users.Where(u => u.Id == id).ExecuteUpdate(s => s.SetProperty(u => u.Username, u => newUsername));
        }
    }
}
