namespace AnotherBlogEngine.Core.Providers
{
    public class TagDataItem : DataItemBase
    {
        public string? Key { get; set; }

        public string? Value { get; set; }

        public bool Deleted { get; set; }
    }
}
