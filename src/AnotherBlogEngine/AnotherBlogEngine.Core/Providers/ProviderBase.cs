using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Providers
{
    public abstract class ProviderBase
    {
        protected readonly ILogger? Logger;

        protected readonly IDbContext? DbContext;

        protected ProviderBase(ILogger logger, IDbContext dbContext)
        {
            Logger = logger;
            DbContext = dbContext;
        }
    }
}
