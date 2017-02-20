using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Logging
{
    public interface ILogger : ISystem {
        void LogError(string message);
        void LogWarning(string message);
    }
}