using System;
using System.Collections.Generic;
using ColorBlockJam.Core;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace ColorBlockJam.Views
{
    public class BlockView : MonoBehaviour
    {
        [Inject] 
        private readonly Camera _camera;

        [SerializeField] 
        private float _speed;
        
        [SerializeField] 
        private List<CellView> _cellViewPrefabs;
        
        public Block Block { get; private set; }
        
        private Core.Grid _grid;

        private readonly List<CellView> _cellViews = new();
        
        private Vector2 _startScreen;
        private Vector3 _baseWorld;

        private Direction _direction;

        private float _carryWorld;
        private float _deadzonePx = 10f;

        private Vector3 _targetPosition;

        public event Action<BlockView> Dissolved;
        
        public void Construct(Core.Grid grid, Block block)
        {
            _grid = grid;
            Block = block;

            foreach (CellView cellView in _cellViews)
            {
                cellView.Destroy();
            }

            _cellViews.Clear();

            CellView cellViewPrefab = _cellViewPrefabs.Find(c => c.ColorId == block.ColorId);
            
            foreach (Cell cell in block.Cells)
            {
                CellView cellView = Instantiate(cellViewPrefab, new Vector3(cell.X, cell.Y, 0), Quaternion.identity,
                    transform);
                cellView.Construct(cell);
                
                _cellViews.Add(cellView);
            }
        }

        public void Hold(Vector2 screenPosition)
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.transform.DOKill();
            }
            
            SetHighlightStatus(true);
            
            _startScreen = screenPosition;
            _baseWorld = transform.position;
            _carryWorld = 0f;

            _direction = Direction.Right;
        }

        public void Drag(Vector2 screenPosition)
        {
            Vector2 delta = screenPosition - _startScreen;

            if (delta.magnitude < _deadzonePx)
            {
                _carryWorld = 0f;
                transform.position = _baseWorld;
                return;
            }

            _direction = DetermineDirection(delta);

            float axisPx = AxisPixels(delta, _direction);
            float axisWorld = ScreenDeltaToWorld(axisPx);

            int maxSteps = _grid.GetMaxSteps(Block.Id, _direction);

            float maxWorld = maxSteps;

            float clampedWorld = Mathf.Clamp(axisWorld, 0f, maxWorld);

            int desiredWholeSteps = Mathf.FloorToInt(clampedWorld);
            float desiredCarry = clampedWorld - desiredWholeSteps;

            CommitForwardSteps(desiredWholeSteps);

            _carryWorld = desiredCarry;
            _targetPosition = _baseWorld + DirectionToWorld(_direction) * _carryWorld;
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _speed * Time.deltaTime);
        }

        public void Release()
        {
            _carryWorld = 0f;

            for (int i = 0; i < _cellViews.Count; i++)
            {
                Cell cell = Block.Cells[i];

                CellView cellView = _cellViews[i];
                cellView.Construct(cell);
                cellView.transform.DOKill();
                cellView.transform.DOMove(new Vector3(cell.X, cell.Y, 0), .5f);
            }

            SetHighlightStatus(false);

            Array directions = Enum.GetValues(typeof(Direction));

            for (int i = 0; i < directions.Length; i++)
            {
                Direction direction = (Direction) directions.GetValue(i);

                if (_grid.TryHandleExit(Block, direction))
                {
                    Dissolve(direction);
                    
                    break;
                }
            }
        }

        private void Dissolve(Direction direction)
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.transform.DOKill();
            }
            
            Sequence sequence = DOTween.Sequence();
            
            Vector3 worldDirection = DirectionToWorld(direction);
            
            sequence.Append(transform.DOMove(transform.position + worldDirection * 5, 1.5f).SetEase(Ease.OutQuad));
            
            sequence.OnComplete(() =>
            {
                Dissolved?.Invoke(this);
                
                Destroy();
            });
        }

        private void SetHighlightStatus(bool status)
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.SetHighlightObjectStatus(status);
            }
        }

        private void CommitForwardSteps(int desiredWholeSteps)
        {
            for (int i = 0; i < desiredWholeSteps; i++)
            {
                if (!_grid.TryMoveOneStep(Block.Id, _direction))
                {
                    break;
                }

                _baseWorld += DirectionToWorld(_direction);

                ShiftStartScreenByOneCell(_direction);
            }
        }

        private void ShiftStartScreenByOneCell(Direction direction)
        {
            float worldPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
            if (worldPerPixel <= 0f)
            {
                return;
            }

            float px = 1 / worldPerPixel;

            switch (direction)
            {
                case Direction.Right: _startScreen.x += px; break;
                case Direction.Left: _startScreen.x -= px; break;
                case Direction.Up: _startScreen.y += px; break;
                case Direction.Down: _startScreen.y -= px; break;
            }
        }

        private static Direction DetermineDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return delta.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        private static float AxisPixels(Vector2 delta, Direction direction)
        {
            float axis = (direction == Direction.Left || direction == Direction.Right) ? delta.x : delta.y;
            if (direction == Direction.Left || direction == Direction.Down)
            {
                axis = -axis;
            }

            return axis;
        }

        private static Vector3 DirectionToWorld(Direction direction)
        {
            return direction switch
            {
                Direction.Right => Vector3.right,
                Direction.Left => Vector3.left,
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                _ => Vector3.zero
            };
        }

        private float ScreenDeltaToWorld(float px)
        {
            if (_camera.orthographic)
            {
                float worldPerPixed = (_camera.orthographicSize * 2f) / Screen.height;

                return px * worldPerPixed;
            }

            return px * .01f;
        }
        public void Destroy()
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.Destroy();
            }

            _cellViews.Clear();

            Destroy(gameObject);
        }
    }
}