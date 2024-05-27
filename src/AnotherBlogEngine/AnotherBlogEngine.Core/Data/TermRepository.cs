using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Data
{
    //TODO: These could also be made internal?
    public class TermRepository : RepositoryBase<TermDto>, ITermRepository<TermDto>
    {
        public TermRepository(ILogger logger, IDbContext dbContext) : base(logger, dbContext)
        {
        }
    }
}
