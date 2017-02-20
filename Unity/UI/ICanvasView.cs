using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Unity.UI
{
    public interface ICanvasView : IView {
        void SetHUD(IView hud);
    }
}
