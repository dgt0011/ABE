using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Data
{
    public class TagRepository : RepositoryBase<TagDto>, ITagRepository<TagDto>
    {
        public TagRepository(ILogger logger, IDbContext dbContext) : base(logger, dbContext)
        {
        }
    }
}
