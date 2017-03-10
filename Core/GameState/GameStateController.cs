using System;
using SFuller.SharpGameLibs.Core.IOC;
using System.Text;
using System.Linq;
using SFuller.SharpGameLibs.Core.Logging;

namespace SFuller.SharpGameLibs.Core.GameState {
    
    public class GameStateController : IGameStateController, IInitializable
    { 

        public event Action FailedToTransition;

        public Type[] GetDependencies() {
            return null;
        }

        public void Init(IIOCProvider systems) {
            ILogger logger = systems.Get<ILogger>();
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
            FrameworkSystems.RegisterToContextAsWeak(context);
            _systems.SetContext(context);
            ContainerInitResult result = _systems.Init();
            if (result.Status != ContainerInitStatus.Ok) {
                LogContainerProblems(result);
                HandleFailedToTransition();
            }

            _currentState = _nextState;
            _nextState.ReadyToTransitionIn += HandleGameStateReady;
            _nextState.Enter(_systems);
        }

        private void HandleGameStateReady() {
            _currentState.ReadyToTransitionIn -= HandleGameStateReady;
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

        private void LogContainerProblems(ContainerInitResult result) {
            var builder = new StringBuilder();
            builder.Append("Failed to init IOC container for game state ");
            builder.Append(_nextState.GetType().Name);

            Type[] missing = result.Missing?.ToArray();
            CircularDependency[] chain = result.Circular?.ToArray();

            if (missing?.Length > 0){
                builder.Append("\nMissing dependencies: ");
                for (int i = 0, ilen = missing.Length; i < ilen; ++i) {
                    builder.Append(missing[i].Name);
                    if (i < ilen - 1) {
                        builder.Append(", ");
                    }
                }
            }
            
            if (chain?.Length > 0){
                builder.Append("\nCircular dependency chains: ");
                for (int i = 0, ilen = chain.Length; i < ilen; ++i) {
                    Type[] types = chain[i].Chain.ToArray();
                    for (int j = 0, jlen = types.Length; j < jlen; ++j) {
                        builder.Append(types[j].Name);
                        if (j < jlen - 1) {
                            builder.Append(" -> ");
                        }
                    }
                }
            }

            _logger.LogError(builder.ToString());
        }

        public SystemContainer FrameworkSystems;

        private ILogger _logger;

        private SystemContainer _systems;
        private IGameState _currentState;
        private IGameState _nextState;
        private ITransitionController _currentTransitionController;
    }

}
