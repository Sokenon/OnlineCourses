using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Net.Http;
using OnlineCourses.Servers.Classes;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OnlineCourses.Services.Classes;

namespace OnlineCourses.Controllers
{
    public class AuthController : Controller
    {
        IAuthentificationService authService;
        ITokenService tokenService;
        IUserService userService;

        public AuthController(IAuthentificationService auth, ITokenService token, IUserService user)
        {
            authService = auth;
            tokenService = token;
            userService = user;
        }

        [HttpPost]
        public ActionResult Login([FromBody] LoginDTO body)
        {
            HttpContext httpContext = HttpContext;
            var bannedEmailService = httpContext.RequestServices.GetRequiredService<Redis>();
            if (bannedEmailService.IsEmailDeleted(body.Email))
            {
                httpContext.Response.WriteAsync("Пользователь удалён");
                return new StatusCodeResult(403);
            }
            else
            {
                try
                {
                    User? user = userService.FindUser(body.Email, authService.ValidationInfoAndHashPassword(body));
                    if (user != null)
                    {
                        httpContext.Response.Cookies.Append(AuthService.cookie, new JwtSecurityTokenHandler().WriteToken(tokenService.Token(user!.Id!.ToString(), user!.Role!)));
                        return new StatusCodeResult(200);
                    }
                    else
                    {
                        return new StatusCodeResult(401);
                    }
                }
                catch (Exception)
                {
                    return new StatusCodeResult(401);
                    throw;
                }
            }
        }
        [HttpPost]
        public ActionResult Registration([FromBody] LoginDTO body)
        {
            HttpContext httpContext = HttpContext;
            var bannedEmailService = httpContext.RequestServices.GetRequiredService<Redis>();
            if (bannedEmailService.IsEmailDeleted(body.Email))
            {
                httpContext.Response.WriteAsync("Пользователь удалён");
                return new StatusCodeResult(403);
            }
            else
            {
                try
                {
                    if (!userService.CheckUser(body.Email))
                    {
                        User user = userService.AddNewUser(body.Email, authService.ValidationInfoAndHashPassword(body));
                        httpContext.Response.Cookies.Append(AuthService.cookie, new JwtSecurityTokenHandler().WriteToken(tokenService.Token(user!.Id!.ToString(), user!.Role!)));
                        return new StatusCodeResult(200);
                    }
                    else
                    {
                        return new StatusCodeResult(404);
                    }
                }
                catch (Exception)
                {
                    return new StatusCodeResult(404);
                    throw;
                }
            }
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

    }
}
