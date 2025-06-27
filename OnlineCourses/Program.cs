using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Controllers;
using OnlineCourses.Models;
using System.Text;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Servers.Classes;
using OnlineCourses.Services.Interfaces;
using OnlineCourses.Services.Classes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using OnlineCourses.Middleware;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetSection("AuthOptions").GetValue<string>("ISSUER"),
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("AuthOptions").GetValue<string>("AUDIENCE"),
        ValidateLifetime = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(builder.Configuration.GetSection("AuthOptions").GetValue<string>("KEY")!),
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connection));
builder.Services.AddStackExchangeRedisCache(options =>
{
    var opt = builder.Configuration.GetSection("Redis");
    options.Configuration = $"{opt.GetValue<string>("Host")}:{opt.GetValue<string>("Port")}, password={opt.GetValue<string>("Password")}";
    options.InstanceName = opt.GetValue<string>("InstanceName");
});
builder.Services.AddTransient<ITokenService, JWT>();
builder.Services.AddTransient<IAuthentificationService, AuthService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICourseService, CourseService>();
builder.Services.AddTransient<Redis, Redis>();

var app = builder.Build();

app.UseMiddleware<MyAuthorization>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Course}/{action=Main}");

app.Run();