using System;
using System.Collections.Generic;
using System.Linq;
using ColorBlockJam.Core;
using ColorBlockJam.Models;
using ColorBlockJam.Signals;
using ColorBlockJam.Views;
using UnityEngine;
using Zenject;

namespace ColorBlockJam.Controllers
{
    public class GridController : IInitializable, IDisposable
    {
        [Inject] 
        private readonly IInstantiator _instantiator;
        
        [Inject] 
        private readonly SignalCenter _signalCenter;
        
        [Inject] 
        private readonly CellView _cellViewPrefab;

        [Inject] 
        private readonly BlockView _blockViewPrefab;

        [Inject] 
        private readonly ExitView _exitViewPrefab;

        [Inject] 
        private readonly ObstacleView _obstacleViewPrefab;

        [Inject]
        private readonly Camera _camera;

        [Inject] 
        private readonly List<LevelInfo> _levelInfos;
        
        private readonly List<CellView> _gridCellViews = new();
        
        private readonly List<ExitView> _exitViews = new();
        
        private readonly List<ObstacleView> _obstacleViews = new();
        
        private readonly Dictionary<int, BlockView> _blockViews = new();

        private Core.Grid _grid;

        private int _pickedBlockId;
        
        private LevelInfo _currentLevel;

        public void Initialize()
        {
            ListenEvents();
        }

        private void ListenEvents()
        {
            _signalCenter.Subscribe<StartGameRequestedSignal>(OnStartGameRequested);
            _signalCenter.Subscribe<RestartGameRequestedSignal>(OnRestartGameRequested);
            _signalCenter.Subscribe<TouchBeginSignal>(OnTouchBegan);
            _signalCenter.Subscribe<TouchDragSignal>(OnTouchDrag);
            _signalCenter.Subscribe<TouchEndSignal>(OnTouchEnd);
        }

        private void OnStartGameRequested(StartGameRequestedSignal signal)
        {
            _currentLevel = signal.LevelInfo;
            
            ClearLevel();
            
            InitializeBoard(signal.LevelInfo);
        }

        private void OnRestartGameRequested(RestartGameRequestedSignal _)
        {
            ClearLevel();
            
            InitializeBoard(_currentLevel);
        }

        private void OnTouchBegan(TouchBeginSignal signal)
        {
            if(_grid == null)
            {
                return;
            }
            
            _pickedBlockId = PickNearestBlockId(signal.TouchPosition);

            if (_pickedBlockId == -1)
            {
                return;
            }

            if (!_blockViews.TryGetValue(_pickedBlockId, out BlockView pickedBlockView) || !pickedBlockView)
            {
                _pickedBlockId = -1;
                
                return;
            }
            
            pickedBlockView.Hold(signal.TouchPosition);
        }

        private void OnTouchDrag(TouchDragSignal signal)
        {
            if (_pickedBlockId == -1)
            {
                return;
            }
            
            if (!_blockViews.TryGetValue(_pickedBlockId, out BlockView pickedBlockView) || !pickedBlockView)
            {
                _pickedBlockId = -1;
                
                return;
            }

            pickedBlockView.Drag(signal.TouchPosition);
        }

        private void OnTouchEnd(TouchEndSignal _)
        {
            if (_pickedBlockId == -1)
            {
                return;
            }
            
            if (!_blockViews.TryGetValue(_pickedBlockId, out BlockView pickedBlockView) || !pickedBlockView)
            {
                _pickedBlockId = -1;
                
                return;
            }

            pickedBlockView.Release();
        }

        private void InitializeBoard(LevelInfo levelInfo)
        {
            _grid = new Core.Grid(levelInfo.width, levelInfo.height);

            _camera.orthographicSize = levelInfo.width + 2f;
            _camera.transform.position = new Vector3((_grid.Width / 2f) - .5f, (_grid.Height / 2f) - .5f, _camera.transform.position.z);
         
            InitializeGridLayout(_grid);

            _grid.BlockAdded += OnBlockAdded;
            _grid.BlockRemoved += OnBlockRemoved;
            _grid.BlockMoved += OnBlockMoved;
            _grid.BlockExited += OnBlockExited;
            _grid.ExitAdded += OnExitAdded;
            _grid.ObstacleAdded += OnObstacleAdded;

            for (int i = 0; i < levelInfo.obstacles.Length; i++)
            {
                Vector2 obstacle = levelInfo.obstacles[i];
                
                _grid.AddObstacle(Mathf.FloorToInt(obstacle.x), Mathf.FloorToInt(obstacle.y));
            }

            for (int i = 0; i < levelInfo.exitInfos.Length; i++)
            {
                ExitInfo exitInfo = levelInfo.exitInfos[i];
                
                _grid.AddExit(new Exit(exitInfo.side, exitInfo.from, exitInfo.to, exitInfo.colorId));
            }

            for (int i = 0; i < levelInfo.blockInfos.Length; i++)
            {
                BlockInfo blockInfo = levelInfo.blockInfos[i];

                Block block = new Block(i, blockInfo.colorId, blockInfo.cells.Select(c => new Cell(Mathf.FloorToInt(c.x), Mathf.FloorToInt(c.y))));
                
                _grid.AddBlock(block);
            }
        }

        private void InitializeGridLayout(Core.Grid grid)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    CellView cellView = _instantiator.InstantiatePrefabForComponent<CellView>(_cellViewPrefab, new Vector3(x, y, 0), Quaternion.identity, null);
                    
                    _gridCellViews.Add(cellView);
                }
            }
        }

        private void ClearLevel()
        {
            foreach (ExitView exitView in _exitViews)
            {
                exitView.Destroy();
            }

            _exitViews.Clear();

            foreach (ObstacleView obstacleView in _obstacleViews)
            {
                obstacleView.Destroy();
            }
            
            _obstacleViews.Clear();
            
            foreach (CellView gridCellView in _gridCellViews)
            {
                gridCellView.Destroy();
            }
            
            _gridCellViews.Clear();

            foreach (KeyValuePair<int, BlockView> blockView in _blockViews)
            {
                blockView.Value.Destroy();
            }
            
            _blockViews.Clear();

            if (_grid != null)
            {
                _grid.BlockAdded -= OnBlockAdded;
                _grid.BlockRemoved -= OnBlockRemoved;
                _grid.BlockMoved -= OnBlockMoved;
                _grid.BlockExited -= OnBlockExited;
                _grid.ExitAdded -= OnExitAdded;
                _grid.ObstacleAdded -= OnObstacleAdded;
   
                _grid = null;
            }

            _pickedBlockId = -1;
        }

        private void OnObstacleAdded(int x, int y)
        {
            ObstacleView obstacleView = _instantiator.InstantiatePrefabForComponent<ObstacleView>(_obstacleViewPrefab);
            obstacleView.transform.position = new Vector3(x, y, 0);
            
            _obstacleViews.Add(obstacleView);
        }

        private void OnExitAdded(Exit exit)
        {
            ExitView exitView = _instantiator.InstantiatePrefabForComponent<ExitView>(_exitViewPrefab);
            exitView.Construct(exit, _grid.Width, _grid.Height);
            
            _exitViews.Add(exitView);
        }

        private void OnBlockExited(Block block, Exit exit)
        {
            ExitView exitView = _exitViews.Find(e => e.Exit == exit);
            
            exitView.PlayParticleSystem();
        }

        private void OnBlockMoved(Block block)
        {
            if (block.Id == _pickedBlockId)
            {
                return;
            }
            _blockViews[block.Id].Construct(_grid, block);
        }

        private void OnBlockAdded(Block block)
        {
            BlockView blockView = _instantiator.InstantiatePrefabForComponent<BlockView>(_blockViewPrefab);
            blockView.Construct(_grid, block);

            blockView.Dissolved += OnBlockViewDissolved;
            
            _blockViews.Add(block.Id, blockView);
        }

        private void OnBlockViewDissolved(BlockView blockView)
        {
            blockView.Dissolved -= OnBlockViewDissolved;
            
            _blockViews.Remove(blockView.Block.Id);

            if (_blockViews.Count == 0)
            {
                _signalCenter.Fire(new LevelCompletedSignal());
            }
        }

        private void OnBlockRemoved(Block block)
        {
            _pickedBlockId = -1;
        }

        private int PickNearestBlockId(Vector2 screenPosition)
        {
            Vector2 world = _camera.ScreenToWorldPoint(screenPosition);

            int x = Mathf.RoundToInt(world.x);
            int y = Mathf.RoundToInt(world.y);

            if (x < 0 || x >= _grid.Width || y < 0 || y >= _grid.Height)
            {
                return -1;
            }

            return _grid.GetOccupancy(new Cell(x, y));
        }


        private void UnsubscribeFromEvents()
        {
            _signalCenter.Unsubscribe<StartGameRequestedSignal>(OnStartGameRequested);
            _signalCenter.Unsubscribe<RestartGameRequestedSignal>(OnRestartGameRequested);
            _signalCenter.Unsubscribe<TouchBeginSignal>(OnTouchBegan);
            _signalCenter.Unsubscribe<TouchDragSignal>(OnTouchDrag);
            _signalCenter.Unsubscribe<TouchEndSignal>(OnTouchEnd);
        }
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}