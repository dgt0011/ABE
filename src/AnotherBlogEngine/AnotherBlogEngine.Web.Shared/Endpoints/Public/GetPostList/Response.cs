using AnotherBlogEngine.Web.Shared.Models;

namespace AnotherBlogEngine.Web.Shared.Endpoints.Public.GetPostList
{
    public class Response
    {
        public int TotalPosts { get; set; }

        public List<PostSummaryItem>? Posts { get; set; }
    }
}
