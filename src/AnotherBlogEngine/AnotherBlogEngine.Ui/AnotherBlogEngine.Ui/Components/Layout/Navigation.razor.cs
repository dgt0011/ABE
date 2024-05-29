using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Core.Providers;
using Microsoft.AspNetCore.Components;

namespace AnotherBlogEngine.Ui.Components.Layout
{
    public partial class Navigation
    {
        [Inject]
        protected ITermProvider? TermProvider { get; set; }

        [Inject]
        protected ITopicProvider? TopicProvider { get; set; }

        private List<TopicDataItem>? _topics;

        private string? _topicsLabel;
        private string? _searchLabel;

        protected override async Task OnInitializedAsync()
        {
            if (TopicProvider is not null)
            {
                _topics = await TopicProvider.Get();
            }

            if (TermProvider is not null)
            {
                _topicsLabel = (await TermProvider.Get("TopicsLabel")).Value;
                _searchLabel = (await TermProvider.Get("SearchLabel")).Value;
            }
        }
    }
}
