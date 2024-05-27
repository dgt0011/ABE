namespace AnotherBlogEngine.Core.Data.Interfaces
{
    public interface ITagRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity, new()
    {
    }
}
