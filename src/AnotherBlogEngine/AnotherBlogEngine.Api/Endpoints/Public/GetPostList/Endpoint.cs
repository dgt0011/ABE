using AnotherBlogEngine.Core;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Web.Shared.Endpoints.Public.GetPostList;
using AnotherBlogEngine.Web.Shared.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnotherBlogEngine.Api.Endpoints.Public.GetPostList
{
    public class Endpoint : Endpoint<Request, Results<Ok<Response>, NotFound>>
    {
        public IPostProvider? PostProvider { get; init; }

        public override void Configure()
        {
            Get("publishedposts");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Retrieves all published posts";
                s.Description = "Retrieves published posts.  Draft/unpublished posts are not included.";
            });

        }

        public override async Task<Results<Ok<Response>, NotFound>> ExecuteAsync(Request request, CancellationToken ct)
        {
            Logger.LogDebug("Fetching {PostCount} Post Summaries starting from position {StartIndex}", request.PostCount, request.StartIndex);

            var response = new Response
            {
                TotalPosts = 0,
                Posts = new List<PostSummaryItem>()
            };

            if (PostProvider != null)
            {
                response.TotalPosts = await PostProvider.GetPostCount(Constants.PostStatus.Published);
                response.Posts = await PostProvider.GetPostSummaries(Constants.PostStatus.Published, request.PostCount, request.StartIndex);
            }

            return TypedResults.Ok(response);
        }
    }
}
