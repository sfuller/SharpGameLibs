using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.ViewManagement
{
	public interface IViewManager {
        T Instantiate<T>() where T : IView;
        T Instantiate<T>(uint tag) where T : IView;
        void Destroy<T>(T view) where T : IView;
    }
}
