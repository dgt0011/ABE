using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Web.Shared.Models;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Providers
{
    public class PostProvider : ProviderBase, IPostProvider
    {
        private readonly IPostRepository<PostDetailsDto>? _repository;

        public PostProvider(ILogger logger, IDbContext dbContext, IPostRepository<PostDetailsDto> repository) : base(logger, dbContext)
        {
            _repository = repository;
        }

        public async Task<int> GetPostCount(Constants.PostStatus postStatusFilter)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers), suffix: $"({postStatusFilter})");

            var retVal = await _repository!.GetPostCount(postStatusFilter);

            Logger?.TraceMethodExit(prefix: nameof(Providers), suffix: $"({postStatusFilter})");

            return retVal;
        }

        public async Task<List<PostSummaryDataItem>> GetPostSummaries(Constants.PostStatus postStatusFilter)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers), suffix:$"({postStatusFilter})");

            //TODO: This needs to be instead the shared Model - NOT PostSummaryDataItem
            var retVal = new List<PostSummaryDataItem>();

            var dtos = await _repository!.GetPostSummaries(postStatusFilter);
            foreach (var dto in dtos)
            {
                retVal.Add(Mappings.Instance.Mapper.Map<PostSummaryDataItem>(dto));
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers), suffix: $"({postStatusFilter})");

            return retVal;
        }

        public async Task<List<PostSummaryItem>> GetPostSummaries(Constants.PostStatus postStatusFilter, long postCount, long startIndex)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers), suffix: $"({postStatusFilter})");

            var retVal = new List<PostSummaryItem>();

            var dtos = await _repository!.GetPostSummaries(postStatusFilter, postCount, startIndex);
            foreach (var dto in dtos)
            {
                retVal.Add(Mappings.Instance.Mapper.Map<PostSummaryItem>(dto));
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers), suffix: $"({postStatusFilter})");

            return retVal;
        }

        public async Task<PostItem?> GetPostBySlug(string slug)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers), suffix: $"({slug})");

            var dto = await _repository!.GetPostBySlug(slug);
            if (dto is null)
            {
                return null;
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers), suffix: $"({slug})");
            return Mappings.Instance.Mapper.Map<PostItem>(dto);
        }
    }
}
