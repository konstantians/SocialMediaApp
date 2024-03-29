﻿using Microsoft.AspNetCore.Authentication;
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
    public async Task<IActionResult> SignIn(string returnUrl, bool falseResetAccount, bool invalidCredentials, 
        string externalIdentityProviderError)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");

        SignInModel signInModel = new SignInModel();
        signInModel.ReturnUrl = returnUrl;
        var externalAuthenticationProviders = await _authenticationProcedures.GetExternalIdentityProvidersAsync();
        signInModel.ExternalIdentityProviders = externalAuthenticationProviders.ToList();

        ViewData["FalseResetAccount"] = falseResetAccount;
        ViewData["InvalidCredentials"] = invalidCredentials;
        ViewData["ExternalIdentityProviderError"] = externalIdentityProviderError;
        return View(signInModel);
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
            return RedirectToAction("SignIn", "Account");
        }

        bool result = await _authenticationProcedures.SignInUserAsync(signInModel.Username!, signInModel.Password!, signInModel.RememberMe);
        if (!result)
        {
            return RedirectToAction("SignIn", "Account", new { invalidCredentials = true });
        }

        return RedirectToAction("Index", "Home", new { SuccessfulSignIn = true });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin(string identityProviderName, string returnUrl)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");


        string redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl})!;

        AuthenticationProperties authenticationProperties = _authenticationProcedures
            .GetExternalIdentityProvidersPropertiesAsync(identityProviderName, redirectUrl);

        return new ChallengeResult(identityProviderName, authenticationProperties);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl, string remoteError)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if (appUser is not null)
            return RedirectToAction("Index", "Home");
        
        if(remoteError != null)
            return RedirectToAction("SignIn", "Account", new { externalIdentityProviderError = remoteError });

        string errorCode = await _authenticationProcedures.ExternalSignInUserAsync();
        if (errorCode is not null)
            return RedirectToAction("SignIn", "Account", new { externalIdentityProviderError = errorCode });

        returnUrl = returnUrl ?? Url.Content("~/");
        return LocalRedirect(returnUrl);
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
        bool passwordChangeSuccess, bool passwordChangeError, bool passwordMismatchError,
        bool friendRemovalFailure, bool friendRemovalSuccess)
    {
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

        List<AppUser> userFriends = new();
        foreach (Friendship friendship in appUser.Friendships)
        {
            if(friendship.UserId != appUser.Id)
                userFriends.Add(await _authenticationProcedures.FindByUserIdAsync(friendship.UserId));
            else if(friendship.FriendId != appUser.Id)
                userFriends.Add(await _authenticationProcedures.FindByUserIdAsync(friendship.FriendId));
        }

        EditAccountModel editAccountModel = new()
        {
            UsersFriends = userFriends,
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

        ViewData["FriendRemovalFailure"] = friendRemovalFailure;
        ViewData["FriendRemovalSuccess"] = friendRemovalSuccess;
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

        bool result;
        //if the user has an empty password that means that their account was created with an external identity provider
        if (appUser.PasswordHash is null)
        {
            //valid Guid for password
            string newPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10).Replace('+', 'A').Replace('/', 'B') + "42kK!";

            (bool passwordChangeResult, _) = await _authenticationProcedures.ChangePasswordAsync(appUser, null!, newPassword);
            if (!passwordChangeResult)
                return RedirectToAction("EditAccount", "Account", new { passwordChangeError = true });

            //have to do it again, because otherwise the token is invalid
            resetToken = await _authenticationProcedures.CreateChangeEmailTokenAsync(appUser, changeEmailModel.NewEmail!);

            string message = "Click on the following link to confirm your account's new email:";
            string? link = Url.Action("ConfirmChangeEmail", "Account", new
            {
                userId = WebUtility.UrlEncode(appUser.Id),
                newEmail = changeEmailModel.NewEmail,
                token = WebUtility.UrlEncode(resetToken)
            }, Request.Scheme);

            string? confirmationLink = $"{message} {link}\nWe have also updated your account password so you can login through the login page." +
                $"\nNew Account Password : {newPassword}";
            result = await _emailService.SendEmailAsync(changeEmailModel.NewEmail!, "Email Change Confirmation", confirmationLink);
        }
        else
        {
            string message = "Click on the following link to confirm your account's new email:";
            string? link = Url.Action("ConfirmChangeEmail", "Account", new
            {
                userId = WebUtility.UrlEncode(appUser.Id),
                newEmail = changeEmailModel.NewEmail,
                token = WebUtility.UrlEncode(resetToken)
            }, Request.Scheme);

            string? confirmationLink = $"{message} {link}";
            result = await _emailService.SendEmailAsync(changeEmailModel.NewEmail!, "Email Change Confirmation", confirmationLink);
        }
        
        if (result)
        {
            appUser.EmailConfirmed = false;
            await _authenticationProcedures.UpdateUserAccountAsync(appUser);
            await _authenticationProcedures.LogOutUserAsync();
        }

        ViewData["EmailSendSuccessfully"] = result;
        return View("EmailChangeVerificationMessage");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string token)
    {
        bool succeeded = await _authenticationProcedures.ChangeEmailAsync(userId, WebUtility.UrlDecode(token), newEmail);

        if (!succeeded)
        {
            return RedirectToAction("Index", "Home", new { FailedAccountActivation = true });
        }
        return RedirectToAction("Index", "Home", new { SuccessfulAccountActivation = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddFriend(string username, string email)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AppUser friend;
        if (username is not null)
            friend = await _authenticationProcedures.FindByUsernameAsync(username);
        else
            friend = await _authenticationProcedures.FindByEmailAsync(email);

        if (friend is null)
            return RedirectToAction("EditAccount", "Account", new { userWasNotFound = true });

        if (friend.Id == appUser.Id)
            return RedirectToAction("EditAccount", "Account", new { selfFriendRequest = true });

        if (appUser.Friendships.Any(friendship => friendship.UserId == friend.Id || friendship.FriendId == friend.Id))
            return RedirectToAction("EditAccount", "Account", new { userAlreadyFriend = true});
        //TODO do notifications here
        bool result = await _authenticationProcedures.AddFriend(appUser.Id, friend.Id);
        if(!result)
            return RedirectToAction("EditAccount", "Account", new { friendNotificationSentUnsuccessfully = true });

        return RedirectToAction("EditAccount", "Account", new { friendNotificationSentSuccessfully = true});
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveFriend(string friendId)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        bool result = await _authenticationProcedures.RemoveFriend(appUser.Id, friendId);

        //TODO send notification here, maybe
        if (!result)
            return RedirectToAction("EditAccount", "Account", new { friendRemovalFailure = true });

        return RedirectToAction("EditAccount", "Account", new { friendRemovalSuccess = true });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        await _authenticationProcedures.UpdateUserChatStatus(appUser.Id, null);
        await _authenticationProcedures.LogOutUserAsync();
        return RedirectToAction("Index", "Home");
    }
}
