using AnotherBlogEngine.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static AnotherBlogEngine.Core.Constants;

namespace AnotherBlogEngine.Api.Endpoints
{
    public static class BlogPostEndpoints
    {
        public static void MapPostApi(this WebApplication app)
        {
            app.MapGet("/api/PublishedPosts",
                async (IPostProvider provider, [FromQuery] int postCount, [FromQuery] int startIndex) 
                    => Results.Ok(await provider.GetPostSummaries(PostStatus.Published, postCount, startIndex)));

            app.MapGet("api/PublishedPostCount",
                async (IPostProvider provider)
                    => Results.Ok(await provider.GetPostCount(PostStatus.Published)));

        }
    }
}
