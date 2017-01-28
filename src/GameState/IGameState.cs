using System;
using SFuller.SharpGameLibs.IOC;

namespace SFuller.SharpGameLibs.GameState {
    
    public interface IGameState {

        event Action ReadyToTransitionIn;

        SystemContext GetSystemContext();
        void Enter(SystemContainer systems);
        void Exit();

    }
    
}
