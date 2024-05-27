using AnotherBlogEngine.Core.Data.Interfaces;
using Dapper.Contrib.Extensions;

namespace AnotherBlogEngine.Core.Data.Dto
{
    public abstract class DtoBase : IEntity
    {
        [Key]
        public long id { get; set; }

        public bool deleted_fg { get; set; }
    }
}
