using AnotherBlogEngine.Core.Data.Dto;

namespace AnotherBlogEngine.Core.Data.Interfaces;

public interface IPostRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity, new()
{
    Task<int> GetPostCount(Constants.PostStatus postStatusFilter);

    Task<IReadOnlyCollection<PostSummaryDto>> GetPostSummaries(Constants.PostStatus postStatusFilter);

    Task<IReadOnlyCollection<PostSummaryDto>> GetPostSummaries(Constants.PostStatus postStatusFilter, long count, long startIndex);

    Task<(bool Result, PostDetailsDto? dto)> GetPost(long id);

    Task<TEntity?> GetPostBySlug(string slug);

    Task<(bool Result, PostDetailsDto? dto)> UpsertPost(PostDetailsDto dto);
}