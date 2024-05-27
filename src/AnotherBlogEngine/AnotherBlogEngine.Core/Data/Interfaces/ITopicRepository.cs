namespace AnotherBlogEngine.Core.Data.Interfaces
{
    public interface ITopicRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity, new()
    {
    }
}
