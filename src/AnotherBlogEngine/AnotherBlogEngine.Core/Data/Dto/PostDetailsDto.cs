using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace AnotherBlogEngine.Core.Data.Dto
{
    [Table("blog_post")]
    public sealed class PostDetailsDto : DtoBase, IEquatable<PostDetailsDto>
    {

#pragma warning disable IDE1006

        [Required]
        [StringLength(200)]
        public string? title { get; set; }

        [Required]
        [StringLength(450)]
        public string? description { get; set; }

        [Required]
        [StringLength(450)]
        public string slug { get; set; } = string.Empty;

        public int status { get; set; } = (int)Constants.PostStatus.Draft;

        public int view_count { get; set; }

        public DateTime? published_datetime { get; set; }

        public DateTime? created_datetime { get; set; }

        public string? body { get; set; }

        [Required]
        [StringLength(200)]
        public string cover_img_path { get; set; } = string.Empty;

#pragma warning restore IDE1006

        [Computed]
        public List<TopicDto>? Topics { get; set; }

        [Computed]
        public List<TagDto>? Tags { get; set; }

        public bool Equals(PostDetailsDto? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return title == other.title &&
                   description == other.description &&
                   slug == other.slug &&
                   status == other.status &&
                   view_count == other.view_count &&
                   Nullable.Equals(published_datetime, other.published_datetime) &&
                   Nullable.Equals(created_datetime, other.created_datetime) &&
                   body == other.body &&
                   cover_img_path == other.cover_img_path &&
                   Equals(Topics, other.Topics) &&
                   Equals(Tags, other.Tags) &&
                   deleted_fg == other.deleted_fg;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PostDetailsDto)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(title);
            hashCode.Add(description);
            hashCode.Add(slug);
            hashCode.Add(status);
            hashCode.Add(view_count);
            hashCode.Add(published_datetime);
            hashCode.Add(created_datetime);
            hashCode.Add(body);
            hashCode.Add(cover_img_path);
            hashCode.Add(Topics);
            hashCode.Add(Tags);
            return hashCode.ToHashCode();
        }
    }
}
