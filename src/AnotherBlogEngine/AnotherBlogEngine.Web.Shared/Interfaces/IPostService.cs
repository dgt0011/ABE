namespace AnotherBlogEngine.Web.Shared.Interfaces
{
    public interface IPostService
    {
        Task<Endpoints.Public.GetPostList.Response?> GetPublishedPosts(long postCount, long startIndex);
    }
}
