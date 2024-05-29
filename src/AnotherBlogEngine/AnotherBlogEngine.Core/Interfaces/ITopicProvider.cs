using AnotherBlogEngine.Core.Providers;

namespace AnotherBlogEngine.Core.Interfaces
{
    public interface ITopicProvider
    {

        Task<List<TopicDataItem>> Get();

        Task<TopicDataItem> Get(int topicId);

        Task<bool> Save(TopicDataItem tag);

        Task<bool> Remove(TopicDataItem tag);
    }
}
