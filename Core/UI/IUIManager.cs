using SFuller.SharpGameLibs.Core.ViewManagement;
using System;

namespace SFuller.SharpGameLibs.Core.UI
{
    /// <summary>
    /// Interface to a system which displays a view as the user interface.
    /// </summary>
    public interface IUIManager {
        event Action<object, EventArgs> Ready;
        void Load();
        void SetHUD(IView view, int layer);
    }
}
