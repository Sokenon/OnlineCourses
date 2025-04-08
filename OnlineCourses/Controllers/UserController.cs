using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Models;
using OnlineCourses.Models.ViewsModels;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace OnlineCourses.Controllers
{
    public class UserController : Controller
    {
        ApplicationContext db;
        ITokenService tokenService;
        IUserService userService;
        ICourseService courseService;
        public UserController(ApplicationContext context, ITokenService token, IUserService user, ICourseService course)
        {
            db = context;
            tokenService = token;
            userService = user;
            courseService = course;
        }

        [HttpGet]
        public ActionResult PersonalAccount(int page = 1)
        {
            try
            {
                if (Request.Cookies["Token"] != null)
                {
                    int id = int.Parse(tokenService.GetID(Request.Cookies["Token"]!));
                    User? user = userService.FindUser(id);
                    ViewBag.User = user;

                    int pageSize = 10;
                    var source = courseService.StudentsCourses(id);
                    List<User_Course> complete = courseService.UserCorses(id);
                    ViewBag.IsComtlete = complete;

                    if (source != null)
                    {
                        var count = source.Count();
                        var courses = source.Skip((page - 1) * pageSize).Take(pageSize).ToList<Course>();

                        IndexViewModel viewPage = new IndexViewModel(courses, new PageViewModel(count, page, pageSize));
                        return View(viewPage);
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return Redirect("/auth/login");
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPut]
        public ActionResult SetName([FromBody] User user)
        {
            try
            {
                userService.SetName(user.Username!, int.Parse(tokenService.GetID(Request.Cookies["Token"]!)));
                return StatusCode(200);
            }
            catch (Exception)
            {
                return StatusCode(500);
                throw;
            }
        }
    }
}
