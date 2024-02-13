using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.EmailServiceLibrary;
using SocialMediaApp.MVC.Models;
using SocialMediaApp.MVC.Models.EditAccountModels;
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
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(string username, string email)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        if (username is null)
        {
            appUser = await _authenticationProcedures.FindByEmailAsync(email);
        }
        else
        {
            appUser = await _authenticationProcedures.FindByUsernameAsync(username);
        }

        if (appUser is null)
        {
            return RedirectToAction("SignIn", "Account", new { falseResetAccount = true });
        }

        string resetToken = await _authenticationProcedures.CreateResetPasswordTokenAsync(appUser);

        //maybe do a check here
        string message = "Click on the following link to reset your account password:";
        string? link = Url.Action("ResetPassword", "Account", new
        {
            userId = WebUtility.UrlEncode(appUser.Id),
            token = WebUtility.UrlEncode(resetToken)
        }, Request.Scheme);
        string? confirmationLink = $"{message} {link}";
        ViewData["EmailSendSuccessfully"] = await _emailService.SendEmailAsync(appUser.Email!, "Email Confirmation", confirmationLink);

        return View("ResetPasswordEmailMessage");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string userId, string token)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        appUser = await _authenticationProcedures.FindByUserIdAsync(userId);
        if (appUser is null)
        {
            //TODO Add error here
            return RedirectToAction("Index", "Home");
        }

        ViewData["Username"] = appUser.UserName;
        ViewData["UserId"] = userId;
        ViewData["Token"] = token;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            return View();
        }

        bool succeeded = await _authenticationProcedures.ResetPasswordAsync(
            resetPasswordModel.UserId!, WebUtility.UrlDecode(resetPasswordModel.Token!), resetPasswordModel.Password!);


        if (!succeeded)
        {
            return RedirectToAction("Index", "Home", new { failedPasswordReset = true });
        }
        return RedirectToAction("Index", "Home", new { successfulPasswordReset = true });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditAccount(bool duplicateUsernameError, bool duplicateEmailError,
        bool basicInformationChangeError, bool basicInformationChangeSuccess,
        bool passwordChangeSuccess, bool passwordChangeError, bool passwordMismatchError)
    {
        //AppUser appUser = await _userHelperMethods.GetUserWithBankCardsAndTickets();
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AccountBasicSettingsViewModel accountBasicSettingsViewModel = new()
        {
            Username = appUser.UserName,
            PhoneNumber = appUser.PhoneNumber,
        };

        ChangePasswordModel changePasswordModel = new()
        {
            OldPassword = appUser.PasswordHash
        };

        ChangeEmailModel resetEmailModel = new()
        {
            OldEmail = appUser.Email
        };

        EditAccountModel editAccountModel = new()
        {
            AccountBasicSettingsViewModel = accountBasicSettingsViewModel,
            ChangePasswordModel = changePasswordModel,
            ChangeEmailModel = resetEmailModel
        };

        ViewData["DuplicateUsernameError"] = duplicateUsernameError;
        ViewData["DuplicateEmailError"] = duplicateEmailError;

        ViewData["BasicInformationChangeError"] = basicInformationChangeError;
        ViewData["BasicInformationChangeSuccess"] = basicInformationChangeSuccess;

        ViewData["PasswordChangeSuccess"] = passwordChangeSuccess;
        ViewData["PasswordChangeError"] = passwordChangeError;
        ViewData["PasswordMismatchError"] = passwordMismatchError;

        return View(editAccountModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangeBasicAccountSettings(AccountBasicSettingsViewModel accountBasicSettingsViewModel)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        appUser.UserName = accountBasicSettingsViewModel.Username;
        appUser.PhoneNumber = accountBasicSettingsViewModel.PhoneNumber;

        if (appUser.UserName != accountBasicSettingsViewModel.Username)
        {
            if (await _authenticationProcedures.FindByUsernameAsync(accountBasicSettingsViewModel.Username!) is not null)
            {
                return RedirectToAction("EditAccount", "Account", new { duplicateUsernameError = true });
            }
        }

        bool result = await _authenticationProcedures.UpdateUserAccountAsync(appUser);
        if (!result)
            return RedirectToAction("EditAccount", "Account", new { basicInformationChangeError = true });

        return RedirectToAction("EditAccount", "Account", new { basicInformationChangeSuccess = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel changePasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("EditAccount", "Account", new { passwordChangeError = true });
        }

        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();

        (bool result, string errorCode) = await _authenticationProcedures.ChangePasswordAsync(appUser, changePasswordModel.OldPassword!, changePasswordModel.NewPassword!);

        if (!result && errorCode == "passwordMismatch")
            return RedirectToAction("EditAccount", "Account", new { passwordMismatchError = true });
        else if (!result)
            return RedirectToAction("EditAccount", "Account", new { passwordChangeError = true });

        return RedirectToAction("EditAccount", "Account", new { passwordChangeSuccess = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RequestChangeAccountEmail(ChangeEmailModel changeEmailModel)
    {
        if(!ModelState.IsValid)
        {
            return RedirectToAction("EditAccount", "Account");
        }

        AppUser appUser = await _authenticationProcedures.FindByEmailAsync(changeEmailModel.NewEmail!);
        if (appUser is not null)
            return RedirectToAction("EditAccount", "Account", new { duplicateEmailError = true });

        appUser = await _authenticationProcedures.GetCurrentUserAsync();
        string resetToken = await _authenticationProcedures.CreateChangeEmailTokenAsync(appUser, changeEmailModel.NewEmail!);

        //maybe do a check here
        string message = "Click on the following link to confirm your account's new email:";
        string? link = Url.Action("ConfirmChangeEmail", "Account", new
        {
            userId = WebUtility.UrlEncode(appUser.Id),
            newEmail = WebUtility.UrlEncode(changeEmailModel.NewEmail),
            token = WebUtility.UrlEncode(resetToken)
        }, Request.Scheme);
        string? confirmationLink = $"{message} {link}";
        bool result = await _emailService.SendEmailAsync(changeEmailModel.NewEmail!, "Email Change Confirmation", confirmationLink);
        if (result)
        {
            appUser.EmailConfirmed = false;
            await _authenticationProcedures.UpdateUserAccountAsync(appUser);
            await _authenticationProcedures.LogOutUserAsync();
        }
        return RedirectToAction("EmailChangeVerificationMessage", "Account", new { wasSuccessful = result });
    }

    public IActionResult EmailChangeVerificationMessage(bool wasSuccessful)
    {
        ViewData["EmailSendSuccessfully"] = wasSuccessful;
        return View();
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string token)
    {
        bool succeeded = await _authenticationProcedures.ChangeEmailAsync(userId, WebUtility.UrlDecode(token), WebUtility.UrlDecode(newEmail));

        if (!succeeded)
        {
            return RedirectToAction("Index", "Home", new { FailedAccountActivation = true });
        }
        return RedirectToAction("Index", "Home", new { SuccessfulAccountActivation = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        await _authenticationProcedures.LogOutUserAsync();
        return RedirectToAction("Index", "Home");
    }
}
