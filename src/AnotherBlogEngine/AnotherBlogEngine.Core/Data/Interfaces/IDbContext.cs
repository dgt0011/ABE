using System.Data;

namespace AnotherBlogEngine.Core.Data.Interfaces
{
    public interface IDbContext
    {
        // Not entirely sure this is required currently ...?
        IDbConnection? CreateConnection();

        IDbConnection? CreateOpenConnection();
    }
}
