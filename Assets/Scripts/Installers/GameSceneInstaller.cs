using System.Collections.Generic;
using ColorBlockJam.Controllers;
using ColorBlockJam.Models;
using ColorBlockJam.UIs;
using ColorBlockJam.Views;
using UnityEngine;
using Zenject;

namespace ColorBlockJam.Installers
{
    public class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
    {
        [SerializeField] 
        private Camera _mainCamera;

        [SerializeField] 
        private BlockView _blockViewPrefab;
        
        [SerializeField]
        private CellView _gridCellViewPrefab;
        
        [SerializeField]
        private ExitView _exitViewPrefab;
        
        [SerializeField]
        private ObstacleView _obstacleViewPrefab;

        [SerializeField] 
        private List<UIPanel> _uiPanelsPrefabs;
        
        [SerializeField] 
        private List<LevelInfo> _levelInfos;

        public override void InstallBindings()
        {
            Container.BindInstance(_mainCamera).AsSingle();
            Container.BindInstance(_blockViewPrefab).AsSingle();
            Container.BindInstance(_gridCellViewPrefab).AsSingle();
            Container.BindInstance(_exitViewPrefab).AsSingle();
            Container.BindInstance(_obstacleViewPrefab).AsSingle();
            
            Container.BindInstance(_uiPanelsPrefabs).AsSingle();

            Container.BindInstance(_levelInfos).AsSingle();

            Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GridController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UIController>().AsSingle().NonLazy();
        }
    }
}