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
    }
}
