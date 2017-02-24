using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Core.UI
{
    /// <summary>
    /// Interface to a system which displays a view as the user interface.
    /// </summary>
    public interface IUIManager : ISystem {
        void SetHUD(IView view, int layer);
    }
}
