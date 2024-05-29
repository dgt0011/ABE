using AnotherBlogEngine.Core.Providers;
using AnotherBlogEngine.Web.Shared.Models;

namespace AnotherBlogEngine.Core.Interfaces
{
    public interface IPostProvider
    {
        Task<int> GetPostCount(Constants.PostStatus postStatusFilter);

        Task<List<PostSummaryDataItem>> GetPostSummaries(Constants.PostStatus postStatusFilter);

        Task<List<PostSummaryItem>> GetPostSummaries(Constants.PostStatus postStatusFilter, long postCount, long startIndex);

        Task<PostItem?> GetPostBySlug(string slug);
    }
}
