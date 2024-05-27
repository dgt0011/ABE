namespace AnotherBlogEngine.Core.Data.Interfaces
{
    public interface ITermRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity, new()
    {
    }
}
