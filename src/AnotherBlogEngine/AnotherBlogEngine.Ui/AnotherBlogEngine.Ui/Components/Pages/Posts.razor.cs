using AnotherBlogEngine.Core;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AnotherBlogEngine.Ui.Components.Pages
{
    public partial class Posts
    {
        [Inject]
        public IPostProvider? PostProvider { get; set; }

        protected uint TotalPosts { get; set; }

        protected List<PostSummaryItem>? PostSummaries { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (PostProvider != null)
            {
                PostSummaries = await PostProvider.GetPostSummaries(Constants.PostStatus.Published, 10, 0);
            }
        }

        //protected async ValueTask<ItemsProviderResult<PostSummaryItem>> LoadPostSummaries(ItemsProviderRequest request)
        //{
        //    long postCount = 0;
        //    List<PostSummaryItem> retVal = new();

        //    if (PostProvider != null)
        //    {
        //        postCount = await PostProvider.GetPostCount(Constants.PostStatus.Published);
        //        var posts = await PostProvider.GetPostSummaries(Constants.PostStatus.Published, 10, request.StartIndex);


        //        if (posts.Count != 0)
        //        {
        //            retVal = posts;
        //        }
        //        postCount = Math.Min(request.Count, postCount - request.StartIndex);
        //    }

        //    return new ItemsProviderResult<PostSummaryItem>(retVal, (int)postCount);
        //}
    }
}
