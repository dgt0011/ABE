using Microsoft.Extensions.Logging;

namespace AnotherBlogEngine.Core.Data.FunctionalTests
{
    public class FakeLogger : ILogger
    {
        private readonly LogLevel _setLogLevel;
        private readonly List<string> _logs = new();

        internal IExternalScopeProvider? ScopeProvider { get; set; }

        public FakeLogger(LogLevel logLevel)
        {
            _setLogLevel = logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            //if (exception == null) return;

            var str = formatter(state, exception);
            if (string.IsNullOrEmpty(str))
                return;

            var message = $"[{logLevel.ToString().ToUpper()}]: {str}";
            _logs.Add(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _setLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return ScopeProvider?.Push(state) ?? NullScope.Instance;
        }

        internal class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }

        public List<string> LogEntries => _logs;
    }
}
