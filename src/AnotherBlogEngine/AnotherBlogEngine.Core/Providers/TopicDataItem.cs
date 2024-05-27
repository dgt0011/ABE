namespace AnotherBlogEngine.Core.Providers
{
    public class TopicDataItem : DataItemBase
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        public bool Deleted { get; set; }
    }
}
