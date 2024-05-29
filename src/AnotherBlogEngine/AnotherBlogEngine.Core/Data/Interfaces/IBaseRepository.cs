using System.Data;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace AnotherBlogEngine.Core.Data.Interfaces;

/// <summary>
/// Basic repository functionality for CRUD operations.
/// Specialisation of repository functions is left to the specific repositories,
/// whilst base CRUD operations are the BaseRepository responsibility
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : IEntity, new()
{
    ILogger Logger { set; get; }

    Task<TEntity?> Find(long id);

    Task<IEnumerable<TEntity>> FindAll();

    Task<bool> Delete(long id);

    Task<(bool Result, TEntity? Entity)> Upsert(TEntity item, IDbTransaction? transaction = null);

    Task<uint> Count();
}