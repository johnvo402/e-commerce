using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class RedirectAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity.IsAuthenticated && user.IsInRole("Administrator"))
        {
            // Redirect admin users away from the home page
            context.Result = new RedirectToActionResult("Index", "Dashboard", new { area = "Admin"});
        }
    }
}
