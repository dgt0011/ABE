using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using AnotherBlogEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Providers
{
    public class TopicProvider : ProviderBase, ITopicProvider
    {
        private readonly ITopicRepository<TopicDto>? _repository;

        public TopicProvider(ILogger logger, IDbContext dbContext, ITopicRepository<TopicDto> repository) : base(logger, dbContext)
        {
            _repository = repository;
        }

        public async Task<List<TopicDataItem>> Get()
        {
            Logger?.TraceMethodEntry(prefix:nameof(Providers));

            var retVal = new List<TopicDataItem>();

            if (_repository is not null)
            {
                //TODO: implement once the database is upgraded & seeded
                //var dtos = await _repository.FindAll();
                //retVal.AddRange(dtos.Select(topicDto => Mappings.Instance.Mapper.Map<TopicDataItem>(topicDto)));

                retVal.Add(new TopicDataItem { Title="AWS", Description = "AWS Related"});
                retVal.Add(new TopicDataItem { Title = "Things I Learned", Description = "Small items or things I have learned." });
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers));
            //return retVal;

            return await Task.FromResult(retVal);
        }

        public Task<TopicDataItem> Get(int topicId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Save(TopicDataItem tag)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Remove(TopicDataItem tag)
        {
            throw new NotImplementedException();
        }
    }
}
