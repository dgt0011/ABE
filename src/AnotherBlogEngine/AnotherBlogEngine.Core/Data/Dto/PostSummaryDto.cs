using Dapper.Contrib.Extensions;

namespace AnotherBlogEngine.Core.Data.Dto
{
    [Table("blog_post")]
    public class PostSummaryDto : DtoBase, IEquatable<PostSummaryDto>
    {

#pragma warning disable IDE1006

        [Write(false)]
        public string? title { get; set; }

        [Write(false)]
        public string? description { get; set; }

        [Write(false)]
        public string slug { get; set; } = string.Empty;

        [Write(false)]
        public int view_count { get; set; }

        [Write(false)]
        public DateTime published_datetime { get; set; }

        [Write(false)]
        public DateTime? created_datetime { get; set; }

        [Write(false)]
        public int status { get; set; } = (int)Constants.PostStatus.Draft;

        [Write(false)]
        public string cover_img_path { get; set; } = string.Empty;
        

#pragma warning restore IDE1006

        [Computed]
        public IEnumerable<TopicDto>? Topics { get; set; }

        [Computed]
        public IEnumerable<TagDto>? Tags { get; set; }

        public bool Equals(PostSummaryDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return title == other.title && 
                   description == other.description && 
                   slug == other.slug && 
                   view_count == other.view_count && 
                   published_datetime.Equals(other.published_datetime) && 
                   Nullable.Equals(created_datetime, other.created_datetime) && 
                   status == other.status &&
                   Equals(Topics, other.Topics) &&
                   Equals(Tags, other.Tags) &&
                   deleted_fg == other.deleted_fg;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PostSummaryDto)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(title, description, slug, view_count, published_datetime, created_datetime, status);
        }
    }
}
