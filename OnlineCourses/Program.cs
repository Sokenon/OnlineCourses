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

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;


builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connection));
builder.Services.AddTransient<ITokenService, JWT>();
builder.Services.AddTransient<IAuthentificationService, AuthService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICourseService, CourseService>();

var app = builder.Build();

app.UseMiddleware<MyAuthorization>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Course}/{action=Main}");

app.Run();

public class AuthOptions
{
    public const string ISSUER = "OnlineCourses";
    public const string AUDIENCE = "ClientOfCourses";
    const string KEY = "supersecret_secretsecretsecretkey!123mehlolkey";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
