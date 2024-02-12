using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.EmailServiceLibrary;
using SocialMediaApp.MVC.Models;
using SocialMediaApp.SharedModels;
using System.Net;

namespace SocialMediaApp.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly IEmailService _emailService;

    public AccountController(IAuthenticationProcedures authenticationProcedures, IEmailService emailService)
    {
        _authenticationProcedures = authenticationProcedures;
        _emailService = emailService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(bool duplicateUsername, bool duplicateEmail)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        ViewData["DuplicateUsername"] = duplicateUsername;
        ViewData["DuplicateEmail"] = duplicateEmail;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(RegisterModel registerModel)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            return View();
        }

        var result = await _authenticationProcedures.FindByUsernameAsync(registerModel.Username!);
        if (result != null)
        {
            return await SignUp(true, false);
        }

        result = await _authenticationProcedures.FindByEmailAsync(registerModel.Email!);
        if (result != null)
        {
            return await SignUp(false, true);
        }

        appUser = new AppUser();
        appUser.UserName = registerModel.Username;
        appUser.PhoneNumber = registerModel.PhoneNumber;
        appUser.Email = registerModel.Email;

        (string userId, string confirmationToken) = await _authenticationProcedures.RegisterUserAsync(appUser, registerModel.Password!,
            false);
        //maybe do a check here
        string message = "Click on the following link to confirm your email:";
        string? link = Url.Action("ConfirmEmail", "Account", new { userId = WebUtility.UrlEncode(userId), token = WebUtility.UrlEncode(confirmationToken) }, Request.Scheme);
        string? confirmationLink = $"{message} {link}";

        ViewData["EmailSendSuccessfully"] = await _emailService.SendEmailAsync(appUser.Email!, "Email Confirmation", confirmationLink);
        return View("RegisterVerificationEmailMessage");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        bool succeeded = await _authenticationProcedures.ConfirmEmailAsync(userId, WebUtility.UrlDecode(token));

        if (!succeeded)
        {
            return RedirectToAction("Index", "Home", new { FailedAccountActivation = true });
        }
        return RedirectToAction("Index", "Home", new { SuccessfulAccountActivation = true });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(bool falseResetAccount, bool invalidCredentials)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        ViewData["FalseResetAccount"] = falseResetAccount;
        ViewData["InvalidCredentials"] = invalidCredentials;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(SignInModel signInModel)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            return View();
        }

        bool result = await _authenticationProcedures.SignInUserAsync(signInModel.Username!, signInModel.Password!, signInModel.RememberMe);
        if (!result)
        {
            ViewData["FalseResetAccount"] = false;
            ViewData["InvalidCredentials"] = true;
            return View();
        }

        return RedirectToAction("Index", "Home", new { SuccessfulSignIn = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        await _authenticationProcedures.LogOutUserAsync();
        return RedirectToAction("Index", "Home");
    }
}
