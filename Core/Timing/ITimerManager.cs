using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Timing
{
    public delegate void TimerCallback(float signature);

    public interface ITimerManager
    {
        float SetTimer(float duration, TimerCallback callback);
    }
}
