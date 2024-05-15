using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileBgScraper.Models;
using System.ComponentModel.DataAnnotations;

namespace MobileBgScraper.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.Email
                };

                IdentityResult result = await this._userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }

            return this.View(user);
        }

        public IActionResult Login()
        {
            if (this._signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLogin user, string returnurl = null)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = await this._userManager.FindByEmailAsync(user.Email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await this._signInManager.PasswordSignInAsync(appUser, user.Password, false, false);
                    if (result.Succeeded)
                    {
                        return this.Redirect(returnurl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Login Failed: Invalid Email or Password");
            }

            return this.View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            return this.RedirectToAction("Index", "Home");
        }
    }
}
