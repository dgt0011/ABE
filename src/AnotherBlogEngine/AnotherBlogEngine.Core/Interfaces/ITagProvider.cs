using AnotherBlogEngine.Core.Providers;

namespace AnotherBlogEngine.Core.Interfaces
{
    public interface ITagProvider
    {
        Task<List<TagDataItem>> Get();

        Task<TagDataItem> Get(string key);
    }
}
