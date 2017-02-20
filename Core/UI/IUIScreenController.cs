using System;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Core.UI
{
    /// <summary>
    /// Controls a user interface screen. Managed by a IUIScreenManager.
    /// </summary>
    public interface IUIScreenController {
        
        /// <summary>
        /// Invoked when the screen should be closed.
        /// </summary>
        event Action Died;

        /// <summary>
        /// Whether or not the screen should be closed.
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Called when added to the UIScreenManager
        /// </summary>
        void Setup(IUIScreenManager manager);

        /// <summary>
        /// Get the UI view to be displayed for this screen.
        /// </summary>
        IView GetView();

        /// <summary>
        /// Called when the screen is made active and displayed.
        /// </summary>
        void Focus();

        /// <summary>
        /// Called when the screen is made inactive and not displayed.
        /// </summary>
        void Unfocus();

        /// <summary>
        /// Called when the screen is removed from the UIScreenManager.
        /// </summary>
        void Shutdown();
    }
}