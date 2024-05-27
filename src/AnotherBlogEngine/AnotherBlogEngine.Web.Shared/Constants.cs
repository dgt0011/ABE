namespace AnotherBlogEngine.Web.Shared
{
    public class Constants
    {
        // this is duplicated from Core - this needs to be tidied.  Core cant reference Shared tho.  And Shared certainly cant reference Core
        // possibly relocate to another shared assembly?  Eww.
        public enum PostStatus
        {
            Draft,
            Published,
            Removed
        }
    }
}
