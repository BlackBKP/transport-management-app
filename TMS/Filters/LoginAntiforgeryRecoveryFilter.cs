using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TMS.Filters
{
    // Recover only the sign-in form; never execute or replay a rejected POST.
    public class LoginAntiforgeryRecoveryFilter : IAlwaysRunResultFilter
    {
        private readonly ILogger<LoginAntiforgeryRecoveryFilter> _logger;

        public LoginAntiforgeryRecoveryFilter(ILogger<LoginAntiforgeryRecoveryFilter> logger)
        {
            _logger = logger;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not IAntiforgeryValidationFailedResult)
            {
                return;
            }

            _logger.LogWarning(
                "Sign-in form token validation failed. Returning a fresh login form. " +
                "If this repeats across fresh forms, check shared Data Protection keys.");

            context.HttpContext.Response.Headers.CacheControl = "no-store";

            context.Result = new RedirectToActionResult(
                "Login",
                "Account",
                new { formExpired = true });
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
