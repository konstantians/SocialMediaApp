﻿using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.MVC.Models;
using System.Diagnostics;

namespace SocialMediaApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(bool successfulSignIn, bool failedAccountActivation, bool successfulAccountActivation,
            bool successfulPasswordReset, bool failedPasswordReset)
        {
            ViewData["SuccessfulSignIn"] = successfulSignIn;
            ViewData["FailedAccountActivation"] = failedAccountActivation;
            ViewData["SuccessfulAccountActivation"] = successfulAccountActivation;
            ViewData["SuccessfulPasswordReset"] = successfulPasswordReset;
            ViewData["FailedPasswordReset"] = failedPasswordReset;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}