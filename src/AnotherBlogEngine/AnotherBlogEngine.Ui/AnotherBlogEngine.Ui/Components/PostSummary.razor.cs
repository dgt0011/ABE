using Microsoft.AspNetCore.Components;

namespace AnotherBlogEngine.Ui.Components
{
    public partial class PostSummary
    {
        [Parameter]
        public Web.Shared.Models.PostSummaryItem? Summary { get; set; }
    }
}
