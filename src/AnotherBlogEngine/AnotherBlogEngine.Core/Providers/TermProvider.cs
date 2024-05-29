using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using AnotherBlogEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Providers
{
    public class TermProvider : ProviderBase, ITermProvider
    {
        private readonly ITermRepository<TermDto>? _repository;

        private Dictionary<string, TermDataItem>? _terms;

        public TermProvider(ILogger logger, IDbContext dbContext, ITermRepository<TermDto> repository) : base(logger, dbContext)
        {
            _repository = repository;
        }

        public async Task<List<TermDataItem>> Get()
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers));

            if (_terms is not null)
            {
                Logger?.LogDebug("Terms already read from data store.");
                Logger?.TraceMethodExit(prefix: nameof(Providers));
                return await Task.FromResult(_terms.Values.ToList());
            }

            _terms = new Dictionary<string, TermDataItem>();

            // use the repository to read all the terms from the table
            var dtos = await _repository!.FindAll();
            foreach (var dto in dtos)
            {
                var dataItem = Mappings.Instance.Mapper.Map<TermDataItem>(dto);
                if (dataItem is not null)
                {
                    _terms.Add(dataItem.Key!, dataItem);
                }
            }

            Logger?.TraceMethodExit(prefix: nameof(Providers));
            return await Task.FromResult(_terms.Values.ToList());
        }

        public async Task<TermDataItem> Get(string key)
        {
            Logger?.TraceMethodEntry(prefix: nameof(Providers));

            // ensure that the terms dictionary is primed
            _ = await Get();

            if (_terms!.TryGetValue(key, out var value))
            {
                Logger?.TraceMethodExit(prefix: nameof(Providers));
                return value;
            }

            Logger?.LogInformation($"Unknown term '{key}' requested.");
            Logger?.TraceMethodExit(prefix: nameof(Providers));
            return new TermDataItem { Key = key, Value = "Unknown term" };
        }

    }
}
