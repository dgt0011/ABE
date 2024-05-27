using AnotherBlogEngine.Core.Providers;

namespace AnotherBlogEngine.Core.Interfaces
{
    public interface ITopicProvider
    {

        Task<List<TopicDataItem>> Get();

        //Task<List<CategoryItem>> SearchTopics(string term);

        Task<TopicDataItem> Get(int topicId);

        Task<bool> Save(TopicDataItem tag);

        //Task<TopicDataItem> SaveTopic(string tag);

        //Task<ICollection<TopicDataItem>> GetPostTopics(int postId);
        //Task<bool> AddPostCategory(int postId, string tag);
        //Task<bool> SavePostCategories(int postId, List<Category> categories);

        Task<bool> Remove(TopicDataItem tag);
    }
}
