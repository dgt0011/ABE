using AnotherBlogEngine.Core.Providers;

namespace AnotherBlogEngine.Core.Interfaces
{
    public interface ITermProvider
    {
        Task<List<TermDataItem>> Get();

        Task<TermDataItem> Get(string key);
    }
}
