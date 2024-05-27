namespace AnotherBlogEngine.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFriendlyShortDateString(this DateTime date)
        {
            return $"{date:MMM dd}, {date.Year}";
        }
    }
}
