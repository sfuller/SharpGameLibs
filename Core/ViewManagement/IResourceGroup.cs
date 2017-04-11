using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.ViewManagement
{
    public interface IResourceGroupHandle {
        event Action<object, LoadedEventArgs> Loaded;
        void Load();
        void Unload();
    }
}
