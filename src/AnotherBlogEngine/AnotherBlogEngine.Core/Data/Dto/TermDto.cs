using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace AnotherBlogEngine.Core.Data.Dto
{
    [Table("term")]

    //TODO: These DTO can probably be internal?
    public sealed class TermDto : DtoBase, IEquatable<TermDto>
    {
#pragma warning disable IDE1006
        [Required]
        [StringLength(50)]
        public string? key { get; set; }

        [Required]
        [StringLength(250)]
        public string? text { get; set; }
#pragma warning restore IDE1006
        public bool Equals(TermDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return key == other.key && 
                   text == other.text && 
                   deleted_fg == other.deleted_fg;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TermDto)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(key, text);
        }
    }
}
