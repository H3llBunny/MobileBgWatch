﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Models;
using MobileBgWatch.Services;
using System.Security.Claims;

namespace MobileBgWatch.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUsersService _usersService;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager,
            IUsersService usersService)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
            this._usersService = usersService;
        }

        public IActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (this.ModelState.IsValid)
            {
                ApplicationUser appUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = user.UserName,
                    Email = user.Email
                };

                IdentityResult result = await this._userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    return this.RedirectToAction(nameof(this.Login));
                }
                else
                {
                    foreach (var error in result.Errors)
                        this.ModelState.AddModelError("", error.Description);
                }
            }

            return this.View(user);
        }

        public IActionResult Login(string returnUrl = null)
        {
            if (this._signInManager.IsSignedIn(User))
            {
                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }

            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLogin user, string returnUrl = null)
        {
            if (this.ModelState.IsValid)
            {
                ApplicationUser appUser = await this._userManager.FindByEmailAsync(user.Email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await this._signInManager.PasswordSignInAsync(appUser, user.Password, false, false);
                    if (result.Succeeded)
                    {
                        return this.Redirect(returnUrl ?? "/");
                    }
                }

                this.ModelState.AddModelError("", "Login Failed: Invalid Email or Password");
            }

            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Authorize]
        public IActionResult UserProfile()
        {
            return this.View();
        }


        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userManager.FindByNameAsync(User.Identity.Name);
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                bool passwordCheck = await this._userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (passwordCheck)
                {
                    bool isNewPasswordDifferent = await this._usersService.NewPasswordCheckAsync(userId, model.NewPassword);
                    if (isNewPasswordDifferent)
                    {
                        await this._usersService.UpdatePasswordAsync(userId, model.NewPassword);
                        this.ViewBag.Message = "Password changed successfully!";
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, "New password must be different from old password.");
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "Incorrect Password");
                }
            }

            return this.View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult PersonalData()
        {
            return this.View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError("", error.Description);
                }
                return this.RedirectToAction(nameof(this.PersonalData));
            }
        }
    }
}
