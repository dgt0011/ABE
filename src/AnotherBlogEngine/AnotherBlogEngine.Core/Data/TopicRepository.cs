using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Data
{
    public class TopicRepository : RepositoryBase<TopicDto>, ITopicRepository<TopicDto>
    {
        public TopicRepository(ILogger logger, IDbContext dbContext) : base(logger, dbContext)
        {
        }
    }
}
