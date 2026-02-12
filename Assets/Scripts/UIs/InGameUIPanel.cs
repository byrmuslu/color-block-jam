using ColorBlockJam.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ColorBlockJam.UIs
{
    public class InGameUIPanel : UIPanel
    {
        [Inject]
        private readonly GameController _gameController;

        [SerializeField]
        private Button _restartButton;

        private void Awake()
        {
            ListenEvents();
        }

        private void ListenEvents()
        {
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        private void OnRestartButtonClicked()
        {
            _gameController.RestartGame();
        }

        private void UnsubscribeFromEvents()
        {
            _restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}