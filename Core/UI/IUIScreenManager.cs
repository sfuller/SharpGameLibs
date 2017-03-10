
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.UI
{
    /// <summary>
    /// Manages transitions and history of UI screens.
    /// </summary>
    public interface IUIScreenManager {

        /// <summary>
        /// Adds a screen and displays it immediatley, pushing the current
        /// screen onto the stack.
        /// </summary>
         void Push(IUIScreenController screen);
    }
}