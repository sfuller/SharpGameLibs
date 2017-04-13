using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Logging
{
    public interface ILogger {
        void LogInfo(string message);
        void LogError(string message);
        void LogWarning(string message);
    }
}