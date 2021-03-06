using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.GameState {
    
    public interface IGameStateController : ISystem {

        event Action FailedToTransition;

        void TransitionTo(IGameState state, ITransitionController transition);
    }
    
}
