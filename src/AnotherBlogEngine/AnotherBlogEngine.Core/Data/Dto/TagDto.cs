using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace AnotherBlogEngine.Core.Data.Dto
{
    [Table("tag")]
    public sealed class TagDto : DtoBase, IEquatable<TagDto>
    {
#pragma warning disable IDE1006
        [Required]
        [StringLength(50)]
        public string? title { get; set; }

        [Required]
        [StringLength(250)]
        public string? description { get; set; }

        public DateTime? date_created { get; set; }

        public DateTime? date_updated { get; set; }

#pragma warning restore IDE1006

        public bool Equals(TagDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return title == other.title &&
                   description == other.description &&
                   Nullable.Equals(date_created, other.date_created) &&
                   Nullable.Equals(date_updated, other.date_updated) &&
                   deleted_fg == other.deleted_fg;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TagDto)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(title, description, date_created, date_updated, deleted_fg);
        }
    }


}
