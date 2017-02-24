using System;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Core.UI
{
    public interface IParentView : IView
    {
        void AddChild(IView view);
    }
}
