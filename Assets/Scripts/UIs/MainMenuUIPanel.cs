using System;
using ColorBlockJam.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ColorBlockJam.UIs
{
    public class MainMenuUIPanel : UIPanel
    {
        [Inject] 
        private readonly GameController _gameController;
        
        [SerializeField]
        private Button _startGameButton;

        private void Awake()
        {
            ListenEvents();
        }

        private void ListenEvents()
        {
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        private void OnStartGameButtonClicked()
        {
            _gameController.StartGame();
        }

        private void UnsubscribeFromEvents()
        {
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}