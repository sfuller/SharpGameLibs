using System;


namespace SFuller.SharpGameLibs.Core.GameState {
    
    public interface ITransitionController {

        event Action TransitionOutFinished;
        event Action TransitionInFinished;

        void Setup();
        void StartTransitionOut();
        void StartTransitionIn();
        void Shutdown();
    }

}
