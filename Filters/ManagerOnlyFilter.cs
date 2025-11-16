using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PMSystem.Filters
{
    public class ManagerOnlyFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            
            if (role != "Manager")
            {
                context.Result = new ObjectResult(new 
                { 
                    message = "Access denied. Manager role required." 
                })
                {
                    StatusCode = 403
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed
        }
    }
}