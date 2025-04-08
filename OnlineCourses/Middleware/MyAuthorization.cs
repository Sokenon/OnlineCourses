using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Servers.Interfaces;
using System.Security.Claims;

namespace OnlineCourses.Middleware
{
    public class MyAuthorization
    {
        private readonly RequestDelegate next;
        private readonly ITokenService tokenService;
        public MyAuthorization(RequestDelegate next, ITokenService service)
        {
            this.next = next;
            this.tokenService = service;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/Auth/Login" && context.Request.Path != "/Auth/Registration")
            {
                if (context.Request.Cookies["Token"] != null)
                {
                    SecurityToken validatedToken;
                    ClaimsPrincipal principial = tokenService.Validation(context.Request.Cookies["Token"]!.ToString(), out validatedToken);
                    if (principial.Identity.IsAuthenticated)
                    {
                        //return;
                        await next.Invoke(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        context.Response.Redirect("/Auth/Login");

                    }
                }
                else
                { 
                    context.Response.StatusCode = 401;
                    context.Response.Redirect("/Auth/Login");
                }
            }
            else
            {
                await next.Invoke(context);
            }
        }
    }
}         
