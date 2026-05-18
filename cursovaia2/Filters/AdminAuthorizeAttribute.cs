using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cursovaia2.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!AuthSession.IsAdmin(context.HttpContext.Session))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
