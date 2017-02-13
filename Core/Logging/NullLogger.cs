

namespace SFuller.SharpGameLibs.Core
{
    public class NullLogger : ILogger {
        public void LogError(string message) { }
        public void LogWarning(string message) { }
    }
}