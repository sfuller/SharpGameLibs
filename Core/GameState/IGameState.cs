using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.GameState {
    
    public interface IGameState {

        event Action ReadyToTransitionIn;

        Context GetSystemContext();
        void Enter(IOCContainer systems);
        void Exit();

    }
    
}
