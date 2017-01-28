using System;


namespace SFuller.SharpGameLibs {
    
    public interface ITransitionController {

        event Action TransitionOutFinished;
        event Action TransitionInFinished;

        void Setup();
        void StartTransitionOut();
        void StartTransitionIn();
        void Shutdown();
    }

}
