using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Update
{
    public interface IUpdateManager {
        void Register(IUpdatable updatable);
        void Unregister(IUpdatable updatable);

        void SetTimescale(float scale);
    }
}
