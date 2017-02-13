using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Core.UI
{
    public interface IHUDManager : ISystem
    {
        void SetHUD(IView view);
    }
}
