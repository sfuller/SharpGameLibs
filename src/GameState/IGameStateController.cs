using System;
using SFuller.SharpGameLibs.IOC;

namespace SFuller.SharpGameLibs.GameState {
    
    public interface IGameStateController : ISystem {

        event Action FailedToTransition;

        void TransitionTo(IGameState state, ITransitionController transition);
    }
    
}
