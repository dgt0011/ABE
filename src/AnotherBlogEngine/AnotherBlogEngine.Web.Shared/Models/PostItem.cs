using System.ComponentModel.DataAnnotations;

namespace AnotherBlogEngine.Web.Shared.Models
{
    public class PostItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(450)]
        public string Description { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public int ViewCount { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime PublishedDateTime { get; set; }

        public Constants.PostStatus Status { get; set; }

        public string CoverImagePath { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}
