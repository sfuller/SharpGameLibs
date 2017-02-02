using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.GameState {
    
    public interface IGameState {

        event Action ReadyToTransitionIn;

        SystemContext GetSystemContext();
        void Enter(SystemContainer systems);
        void Exit();

    }
    
}
