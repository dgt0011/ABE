using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using AnotherBlogEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Providers
{
    public class TagProvider : ProviderBase, ITagProvider
    {
        private readonly ITagRepository<TagDto>? _repository;

        private Dictionary<string, TagDataItem>? _tags;

        public TagProvider(ILogger logger, IDbContext dbContext, ITagRepository<TagDto> repository) : base(logger, dbContext)
        {
            _repository = repository;
        }

        public async Task<List<TagDataItem>> Get()
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers));

            if (_tags is not null)
            {
                Logger?.LogDebug("Tags already read from data store.");
                Logger?.TraceMethodExit(prefix: nameof(Providers));
                return await Task.FromResult(_tags.Values.ToList());
            }

            _tags = new Dictionary<string, TagDataItem>();

            // use the repository to read all the terms from the table
            var dtos = await _repository!.FindAll();
            foreach (var dto in dtos)
            {
                var dataItem = Mappings.Instance.Mapper.Map<TagDataItem>(dto);
                if (dataItem is not null)
                {
                    _tags.Add(dataItem.Key!, dataItem);
                }
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers));
            return await Task.FromResult(_tags.Values.ToList());
        }

        public async Task<TagDataItem> Get(string key)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers));

            // ensure that the terms dictionary is primed
            _ = await Get();

            if (_tags!.ContainsKey(key))
            {
                Logger?.TraceMethodExit(prefix: nameof(Providers));
                return _tags[key];
            }

            Logger?.LogInformation($"Unknown tag '{key}' requested.");
            Logger?.TraceMethodExit(prefix: nameof(Providers));
            return new TagDataItem { Key = key, Value = "Unknown term" };

        }
    }
}
