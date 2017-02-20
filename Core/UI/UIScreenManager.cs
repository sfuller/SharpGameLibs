using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Core.UI
{
    public class UIScreenManager : IUIScreenManager
    {
        public Type[] GetDependencies() {
            return new Type[] {
                typeof(IUIManager)
            };
        }

        public void Init(SystemContainer container) {
            _huds = container.Get<IUIManager>();
        }

        public void Push(IUIScreenController screen) {
            if (_currentScreen != null) {
                TeardownCurrentScreen();
                _screens.Push(_currentScreen);
            }
            screen.Setup(this);
            SetupScreen(screen);
        }

        public void Shutdown() {
            if (_currentScreen != null) {
                TeardownCurrentScreen();
                _currentScreen.Shutdown();
            }
            while (_screens.Count > 0) {
                IUIScreenController screen = _screens.Pop();
                screen.Shutdown();
            }
        }

        private void TeardownCurrentScreen() {
            _currentScreen.Died -= HandleCurrentScreenKilled;
            _currentScreen.Unfocus();
        }

        private IUIScreenController GetNextScreen() {
            while (_screens.Count > 0) {
                IUIScreenController screen = _screens.Pop();
                if (!screen.IsDead) {
                    return screen;
                }
                else {
                    screen.Shutdown();
                }
            }

            return null;
        }

        private void SetupScreen(IUIScreenController screen) {
            _currentScreen = screen;
            IView view = screen.GetView();
            _huds.SetHUD(view);
            screen.Focus();
            screen.Died += HandleCurrentScreenKilled;
        }

        private void SetupNextScreen() {
            IUIScreenController screen = GetNextScreen();
            if (screen == null) {
                return;
            }
            SetupScreen(screen);
        }

        private void HandleCurrentScreenKilled() {
            TeardownCurrentScreen();
            _currentScreen.Shutdown();
            SetupNextScreen();
        }

        private IUIManager _huds;

        private IUIScreenController _currentScreen;
        private Stack<IUIScreenController> _screens = new Stack<IUIScreenController>();
    }
}