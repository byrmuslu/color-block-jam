using System;
using System.Collections.Generic;
using ColorBlockJam.Signals;
using ColorBlockJam.UIs;
using Zenject;

namespace ColorBlockJam.Controllers
{
    public class UIController : IInitializable, IDisposable
    {
        private readonly IInstantiator _instantiator;
        
        private readonly SignalCenter _signalCenter;

        private readonly List<UIPanel> _uiPanelsPrefabs;

        private readonly List<UIPanel> _uiPanels;
        
        public UIController(IInstantiator instantiator, SignalCenter signalCenter, List<UIPanel> uiPanelsPrefabs)
        {
            _instantiator = instantiator;
            _signalCenter = signalCenter;
            _uiPanelsPrefabs = uiPanelsPrefabs;
            
            _uiPanels = new ();
            
            ListenEvents();
        }

        public void Initialize()
        {
            OpenUIPanel(typeof(MainMenuUIPanel));
        }

        private void ListenEvents()
        {
            _signalCenter.Subscribe<StartGameRequestedSignal>(OnStartGameRequested);
            _signalCenter.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }


        private void OnStartGameRequested(StartGameRequestedSignal _)
        {
            CloseAllUIPanels();
            
            OpenUIPanel(typeof(InGameUIPanel));
        }
        
        private void OnLevelCompleted(LevelCompletedSignal _)
        {
            CloseAllUIPanels();
            
            OpenUIPanel(typeof(LevelCompleteUIPanel));
        }

        private void OpenUIPanel(Type type)
        {
            if (_uiPanels.Exists(u => u.GetType() == type))
            {
                _uiPanels.Find(u => u.GetType() == type).Activate();
            }
            else
            {
                if (!_uiPanelsPrefabs.Exists(p => p.GetType() == type))
                {
                    throw new ArgumentException($"Invalid UIPanel type. {nameof(type)}");
                }
                
                UIPanel prefab = _uiPanelsPrefabs.Find(u => u.GetType() == type);

                UIPanel uiPanel = _instantiator.InstantiatePrefabForComponent<UIPanel>(prefab);
                
                _uiPanels.Add(uiPanel);
            }
        }

        private void CloseUIPanel(Type type)
        {
            if (_uiPanels.Exists(u => u.GetType() == type))
            {
                _uiPanels.Find(u => u.GetType() == type).Deactivate();
            }
        }

        private void CloseAllUIPanels()
        {
            foreach (UIPanel uiPanel in _uiPanels)
            {
                uiPanel.Deactivate();
            }
        }

        private void UnsubscribeFromEvents()
        {
            _signalCenter.Unsubscribe<StartGameRequestedSignal>(OnStartGameRequested);
            _signalCenter.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}