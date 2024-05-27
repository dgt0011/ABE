using System.ComponentModel;

namespace AnotherBlogEngine.Web.Shared.Endpoints.Public.GetPostList
{
    public class Request
    {
        [DefaultValue(10)]
        public long PostCount { get; set; }

        [DefaultValue(0)]
        public long StartIndex { get; set; }
    }
}
