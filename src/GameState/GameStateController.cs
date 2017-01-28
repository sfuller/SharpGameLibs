using System;
using SFuller.SharpGameLibs.IOC;

namespace SFuller.SharpGameLibs.GameState {
    
    public class GameStateController : IGameStateController { 

        public event Action FailedToTransition;

        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer systems) {
            ILogger logger = systems.Get<ILoggerSystem>();
            if (logger == null) {
                logger = new NullLogger();
            }
            _logger = logger;
        }
        
        public void Shutdown() {
        }

        public void TransitionTo(IGameState state, ITransitionController transition) {
            if (_nextState != null) {
                _logger.LogError(
                    "Cannot start transitioning to a new state while another " +
                    "state transition is in progress!");
                return;
            }
           
            _nextState = state;
            _currentTransitionController = transition;

            transition.Setup();
            transition.TransitionOutFinished += HandleTransitionOutFinished;
            transition.StartTransitionOut();
        }

        private void HandleTransitionOutFinished() {
            _currentTransitionController.TransitionOutFinished -= HandleTransitionOutFinished;
            //_updates.Stop(); 

            if (_currentState != null) {
                _currentState.Exit();
            
                // TODO: This would be a good place for GC if that becomes
                // something we actually need to wory about.

                // Shutdown old systems
                _systems.Shutdown();
            }

            // Setup new systems
            _systems = new SystemContainer();
            SystemContext context = _nextState.GetSystemContext();
            context.RegisterWeak(FrameworkSystems);
            _systems.SetContext(context);
            if (!_systems.Init()) {
                _logger.LogError("Failed to init game state systems");
                HandleFailedToTransition();
            }

            _currentState = _nextState;
            _nextState.ReadyToTransitionIn += HandleGameStateReady;
            _nextState.Enter(_systems);
        }

        private void HandleGameStateReady() {
            _currentState.ReadyToTransitionIn -= HandleGameStateReady;
            //_updates.Start();
            _currentTransitionController.TransitionInFinished += HandleTransitionInFinished;
            _currentTransitionController.StartTransitionIn();
        }

        private void HandleTransitionInFinished() {
            _currentTransitionController.TransitionInFinished -= HandleTransitionInFinished;
            _currentTransitionController.Shutdown();
            _currentTransitionController = null;
            _nextState = null;
        }

        private void HandleFailedToTransition() {
            var handler = FailedToTransition;
            if (handler != null) {
                handler();
            }
        }

        public SystemContainer FrameworkSystems;

        private ILogger _logger;

        private SystemContainer _systems;
        private IGameState _currentState;
        private IGameState _nextState;
        private ITransitionController _currentTransitionController;
    }

}
