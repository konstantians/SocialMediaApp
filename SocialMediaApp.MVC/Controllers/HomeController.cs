using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.MVC.Models;
using SocialMediaApp.MVC.Models.IndexModels;
using SocialMediaApp.SharedModels;
using System.Diagnostics;

namespace SocialMediaApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticationProcedures _authenticationProcedures;
        private readonly IPostDataAccess _postDataAccess;
        
        public HomeController(ILogger<HomeController> logger, IAuthenticationProcedures authenticationProcedures, 
            IPostDataAccess postDataAccess)
        {
            _logger = logger;
            _authenticationProcedures = authenticationProcedures;
            _postDataAccess = postDataAccess;
        }

        public async Task<IActionResult> Index(bool successfulSignIn, bool failedAccountActivation, bool successfulAccountActivation,
            bool successfulPasswordReset, bool failedPasswordReset,
            bool successfulPostCreation, bool failedPostCreation,
            bool successfulPostUpdate, bool failedPostUpdate,
            bool successfulPostDeletion, bool failedPostDeletion)
        {
            ViewData["SuccessfulSignIn"] = successfulSignIn;
            ViewData["FailedAccountActivation"] = failedAccountActivation;
            ViewData["SuccessfulAccountActivation"] = successfulAccountActivation;
            ViewData["SuccessfulPasswordReset"] = successfulPasswordReset;
            ViewData["FailedPasswordReset"] = failedPasswordReset;
            ViewData["SuccessfulPostCreation"] = successfulPostCreation;
            ViewData["FailedPostCreation"] = failedPostCreation;
            ViewData["SuccessfulPostUpdate"] = successfulPostUpdate;
            ViewData["FailedPostUpdate"] = failedPostUpdate;
            ViewData["SuccessfulPostDeletion"] = successfulPostDeletion;
            ViewData["FailedPostDeletion"] = failedPostDeletion;

            IndexViewModel indexViewModel = new IndexViewModel();
            var result = await _postDataAccess.GetPostsAsync(30);
            if(result is not null)
            {
                indexViewModel.Posts = result.ToList();
                foreach (Post post in indexViewModel.Posts)
                {
                    post.AppUser = await _authenticationProcedures.FindByUserIdAsync(post.UserId);
                }
            }

            return View(indexViewModel);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPost(CreatePostModel createPostModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");   
            }

            AppUser user = await _authenticationProcedures.GetCurrentUserAsync();
            Post post = new Post();
            post.Title = createPostModel.Title;
            post.Content = createPostModel.Content;
            post.SentAt = DateTime.Now;
            post.UserId = user.Id;

            var result = await _postDataAccess.CreatePostAsync(post);
            if (result == -1)
                return RedirectToAction("Index", "Home", new {failedPostCreation = true});

            return RedirectToAction("Index", "Home", new {successfulPostCreation = true});
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditPost(EditPostModel editPostModel)
        {
            AppUser user = await _authenticationProcedures.GetCurrentUserAsync();
            var userPosts = await _postDataAccess.GetPostsOfUserAsync(user.Id);
            bool userOwnsPost = userPosts.ToList().Any(post => post.Id == editPostModel.PostId);
            if (!ModelState.IsValid || !userOwnsPost)
            {
                return RedirectToAction("Index", "Home");
            }

            Post post = new Post();
            post.Title = editPostModel.Title;
            post.Content = editPostModel.Content;
            post.SentAt = DateTime.Now;
            post.UserId = user.Id;

            var result = await _postDataAccess.UpdatePostAsync(editPostModel.PostId, post);
            if (!result)
                return RedirectToAction("Index", "Home", new { failedPostUpdate = true });

            return RedirectToAction("Index", "Home", new { successfulPostUpdate = true });

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            AppUser user = await _authenticationProcedures.GetCurrentUserAsync();
            var userPosts = await _postDataAccess.GetPostsOfUserAsync(user.Id);
            bool userOwnsPost = userPosts.ToList().Any(post => post.Id == postId);
            if (!ModelState.IsValid || !userOwnsPost)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _postDataAccess.DeletePostAsync(postId);
            if (!result)
                return RedirectToAction("Index", "Home", new { failedPostDeletion = true });

            return RedirectToAction("Index", "Home", new { successfulPostDeletion = true });
        }
    }
}