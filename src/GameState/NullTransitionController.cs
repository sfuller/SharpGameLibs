using System;

namespace SFuller.SharpGameLibs.GameState {

    public class NullTransitionController : ITransitionController {

        public event Action TransitionOutFinished;
        public event Action TransitionInFinished;

        public void Setup() {
        }

        public void StartTransitionOut() {
            var handler = TransitionOutFinished;
            if (handler != null) {
                handler();
            }
        }

        public void StartTransitionIn() {
            var handler = TransitionInFinished;
            if (handler != null) {
                handler();
            }
        }

        public void Shutdown() {
        }

    }

}
