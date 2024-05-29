using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;


namespace AnotherBlogEngine.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void TraceMethodEntry(this ILogger logger, string? prefix = null, string? suffix = null, [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? caller = null)
        {
            TraceMethod("->", logger, callerFilePath, caller, prefix, suffix);
        }

        public static void TraceMethodExit(this ILogger logger, string? prefix = null, string? suffix = null, [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? caller = null)
        {
            TraceMethod("<-", logger, callerFilePath, caller, prefix, suffix);
        }

        private static string StripPath(string path)
        {
            // var pathWithoutExtension = Path.GetExtension(path);
            var pathWithoutExtension = path.Replace(Path.GetExtension(path), string.Empty) ;
            //account for windows paths on *nix OS
            var separatorIdx = pathWithoutExtension.LastIndexOf('\\');
            return separatorIdx >=0 ? pathWithoutExtension[(separatorIdx+1)..] : pathWithoutExtension;
        }

        private static void TraceMethod(string preamble, ILogger logger, string? callerFilePath = null, string? caller = null, string? prefix = null, string? suffix = null)
        {
            if (string.IsNullOrEmpty(callerFilePath) || string.IsNullOrEmpty(caller))
            {
                return;
            }

            var callerTypeName = StripPath(callerFilePath);
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('.'))
            {
                prefix += ".";
            }
            logger.LogTrace($"{preamble} {prefix}{callerTypeName}.{caller}{suffix}");
        }
    }
}
