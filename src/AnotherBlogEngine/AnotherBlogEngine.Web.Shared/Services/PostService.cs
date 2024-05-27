using AnotherBlogEngine.Web.Shared.Interfaces;

namespace AnotherBlogEngine.Web.Shared.Services
{
    public class PostService : IPostService
    {
        private readonly HttpService _httpService;

        public PostService(HttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<Endpoints.Public.GetPostList.Response?> GetPublishedPosts(long postCount, long startIndex)
        {
            return await _httpService.HttpGet<Endpoints.Public.GetPostList.Response>($"publishedposts?PostCount={postCount}&StartIndex={startIndex}");
        }
    }
}
