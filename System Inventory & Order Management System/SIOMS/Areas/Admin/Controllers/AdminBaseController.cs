using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace SIOMS.Areas.Admin.Controllers
{
    public class AdminBaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Temporary disable for testing
            // var user = context.HttpContext.Session.GetString("AdminUser");
            // if (string.IsNullOrEmpty(user))
            // {
            //     context.Result = new RedirectToActionResult("Login", "Admin", new { area = "Admin" });
            // }

            base.OnActionExecuting(context);
        }
    }
}