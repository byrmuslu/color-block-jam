using ColorBlockJam.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ColorBlockJam.UIs
{
    public class LevelCompleteUIPanel : UIPanel
    {
        [Inject] 
        private readonly GameController _gameController;

        [SerializeField] 
        private Button _nextLevelButton;

        private void Awake()
        {
            ListenEvents();   
        }

        private void ListenEvents()
        {
            _nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        }

        private void OnNextLevelButtonClicked()
        {
            _gameController.NextLevel();
        }

        private void UnsubscribeFromEvents()
        {
            _nextLevelButton.onClick.RemoveListener(OnNextLevelButtonClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}