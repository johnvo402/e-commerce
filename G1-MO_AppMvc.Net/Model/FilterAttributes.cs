using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

public class ValidateSellerAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Cookies["Role"] == null ||
    !string.Equals(context.HttpContext.Request.Cookies["Role"], "c4ca4238a0b923820dcc509a6f75849b", StringComparison.OrdinalIgnoreCase))
        {
            // Chuyển hướng hoặc xử lý lỗi nếu người dùng không hợp lệ
            context.Result = new RedirectResult("/Home/Login");
            return;
        }

        base.OnActionExecuting(context);
    }
}

