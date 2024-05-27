using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AnotherBlogEngine.Ui.Components.Pages
{
    public partial class Post
    {
        [Inject]
        public IPostProvider? PostProvider { get; set; }

        [Parameter]
        public string? Slug { get; set; }

        public PostItem? PostItem { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (PostProvider is not null && ! string.IsNullOrEmpty(Slug))
            {
                PostItem = await PostProvider.GetPostBySlug(Slug);
                await base.OnParametersSetAsync();
            }
        }

    }
}
