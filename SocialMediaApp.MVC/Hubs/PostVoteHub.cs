using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC.Hubs
{
    public class PostVoteHub : Hub
    {
        private readonly IAuthenticationProcedures _authenticationProcedures;
        private readonly IPostDataAccess _postDataAccess;

        public PostVoteHub(IAuthenticationProcedures authenticationProcedures, IPostDataAccess postDataAccess)
        {
            _authenticationProcedures = authenticationProcedures;
            _postDataAccess = postDataAccess;
        }

        [Authorize]
        public async Task<bool> UpdatePostVote(int postId, bool isPositive)
        {
            AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
            Post post = await _postDataAccess.GetPostAsync(postId);

            if(post is null)
            {
                return false;
            }

            PostVote pVote = new PostVote();
            pVote.PostId = postId;
            pVote.IsPositive = isPositive;
            pVote.UserId = appUser.Id;

            int positivePostVoteCount = post.PostVotes.Where(postVote => postVote.IsPositive).Count();
            int negativePostVoteCount = post.PostVotes.Where(postVote => !postVote.IsPositive).Count();
            foreach (PostVote postVote in post.PostVotes)
            {
                //exists but it is different
                if (postVote.UserId == appUser.Id && postVote.IsPositive != isPositive)
                {
                    await _postDataAccess.UpdatePostVoteAsync(postVote.Id, pVote);
                    positivePostVoteCount = isPositive ? positivePostVoteCount + 1 : positivePostVoteCount - 1;
                    negativePostVoteCount = !isPositive ? negativePostVoteCount + 1 : negativePostVoteCount - 1;
                    await Clients.All.SendAsync("PostVotesChanged", postId, positivePostVoteCount, negativePostVoteCount);
                    return true;
                }
                //exists but it is the same(that means the user has just remove the like or the dislike)
                else if(postVote.UserId == appUser.Id && postVote.IsPositive == isPositive)
                {
                    await _postDataAccess.DeletePostVoteAsync(postVote.Id);
                    positivePostVoteCount = isPositive ? positivePostVoteCount - 1 : positivePostVoteCount;
                    negativePostVoteCount = !isPositive ? negativePostVoteCount - 1 : negativePostVoteCount;
                    await Clients.All.SendAsync("PostVotesChanged", postId, positivePostVoteCount, negativePostVoteCount);
                    return true;
                }
            }

            positivePostVoteCount = isPositive ? positivePostVoteCount + 1 : positivePostVoteCount;
            negativePostVoteCount = !isPositive ? negativePostVoteCount + 1 : negativePostVoteCount;
            await Clients.All.SendAsync("PostVotesChanged", postId,positivePostVoteCount, negativePostVoteCount);
            await _postDataAccess.CreatePostVoteAsync(pVote);
            return true;
        }
    }
}
