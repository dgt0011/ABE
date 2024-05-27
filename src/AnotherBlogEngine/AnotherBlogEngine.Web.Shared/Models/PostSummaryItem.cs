
namespace AnotherBlogEngine.Web.Shared.Models;

public class PostSummaryItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int ViewCount { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime PublishedDateTime { get; set; }

    public Constants.PostStatus Status { get; set; }

    public string CoverImagePath { get; set; } = string.Empty;

    public List<TopicItem> Topics { get; set; } = new();

    public List<TagItem> Tags { get; set; } = new();
}