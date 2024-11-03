using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Manager_App.Services;
using Manager_App.Models;

namespace Manager_App.Controllers
{
    public class RegisterController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public RegisterController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public IActionResult LoginPage()
        {
            return View("LoginPage");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginPage", model);
            }

            var user = await _mongoDbService.GetUserAsync(model.Username);
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });

                return RedirectToAction("ManagerHome", "Home");
            }

            ViewData["ErrorMessage"] = "Invalid credentials.";
            return View("LoginPage", model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginPage", model);
            }

            // Сделать генерацию через mail кодов, и запись их в бд
            var validVerificationCodes = new List<string> { "11113333", "33331111" };

            if (!validVerificationCodes.Contains(model.VerificationCode))
            {
                ViewData["ErrorMessage"] = "Invalid verification code.";
                return View("LoginPage", model);
            }

            if (await _mongoDbService.UserExistsAsync(model.Username))
            {
                ViewData["ErrorMessage"] = "Username already taken.";
                return View("LoginPage", model);
            }

            var newUser = new ManagerAccount
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            await _mongoDbService.AddUserAsync(newUser);

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Username) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("ManagerHome", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LoginPage");
        }
    }
}
