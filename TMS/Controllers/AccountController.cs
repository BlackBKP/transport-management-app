using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Filters;
using TMS.Interfaces;
using TMS.Models;

namespace TMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDemoAuthenticationService _authenticationService;

        public AccountController(IDemoAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Login(string? returnUrl = null, bool formExpired = false)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocalPage(returnUrl);
            }

            if (formExpired)
            {
                ViewData["SignInNotice"] =
                    "Your sign-in form expired. Please enter your details again.";
            }

            return View(new LoginModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(LoginAntiforgeryRecoveryFilter))]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Password = string.Empty;
                return View(model);
            }

            string? userName = _authenticationService.Authenticate(model.UserName, model.Password);
            model.Password = string.Empty;

            if (userName == null)
            {
                ModelState.AddModelError(string.Empty, "The username or password is incorrect.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Name, userName)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);

            return RedirectToLocalPage(model.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectToLocalPage(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Transport");
        }
    }
}
