using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorBlockJam.Core
{
    public sealed class Grid
    {
        public readonly int Width;
        public readonly int Height;

        private readonly int[,] _occupancy;

        private readonly Dictionary<int, Block> _blocks = new();
        private readonly List<Exit> _exists = new();

        public IReadOnlyList<Exit> Exits => _exists;
        
        public event Action<Block> BlockAdded;
        public event Action<Block> BlockRemoved;
        public event Action<Block> BlockMoved;
        public event Action<Block, Exit> BlockExited;
        public event Action<Exit> ExitAdded;
        public event Action<int, int> ObstacleAdded;

        public Grid(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException($"Invalid grid size.");
            }
            
            Width = width;
            Height = height;

            _occupancy = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _occupancy[x, y] = -1;
                }
            }
        }

        public void AddExit(Exit exit)
        {
            _exists.Add(exit);
            
            ExitAdded?.Invoke(exit);
        }
        
        public void AddObstacle(int x, int y)
        {
            _occupancy[x, y] = 99;
            
            ObstacleAdded?.Invoke(x, y);
        }
        
        public void AddBlock(Block block)
        {
            if (_blocks.ContainsKey(block.Id))
            {
                throw new InvalidOperationException($"Block id already exists: {block.Id}");
            }

            foreach (Cell cell in block.Cells)
            {
                if (!IsBounds(cell))
                {
                    throw new InvalidOperationException($"Block {block.Id} out of bounds at {cell}");
                }

                if (_occupancy[cell.X, cell.Y] != -1)
                {
                    throw new InvalidOperationException($"Cell {cell} already occupied by block {_occupancy[cell.X, cell.Y]}");
                }
            }

            foreach (Cell cell in block.Cells)
            {
                _occupancy[cell.X, cell.Y] = block.Id;
            }
            
            _blocks.Add(block.Id, block);
            
            BlockAdded?.Invoke(block);
        }

        public void RemoveBlock(int blockId)
        {
            if (!_blocks.TryGetValue(blockId, out Block block))
            {
                return;
            }
            
            foreach (Cell cell in block.Cells)
            {
                _occupancy[cell.X, cell.Y] = -1;
            }

            _blocks.Remove(block.Id);
            
            BlockRemoved?.Invoke(block);
        }

        public int GetMaxSteps(int blockId, Direction direction)
        {
            Block block = GetBlock(blockId);
            
            int steps = 0;

            while (CanStep(block, direction))
            {
                steps++;

                if (!CanStepsOffset(block, direction, steps))
                {
                    return steps - 1;
                }
            }

            return steps;
        }

        private bool CanStepsOffset(Block block, Direction direction, int offsetSteps)
        {
            Cell delta = direction.ToDelta();
            List<Cell> edge = GetLeadingEdge(block.Cells, direction);

            Cell offset = new Cell(delta.X * offsetSteps, delta.Y * offsetSteps);

            foreach (Cell cell in edge)
            {
                Cell t = cell + offset;

                if (!IsBounds(t))
                {
                    return false;
                }

                int other = _occupancy[t.X, t.Y];

                if (other != -1 && other != block.Id)
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool IsBounds(Cell cell)
        {
            return cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;
        }

        public int GetOccupancy(Cell cell)
        {
            if (!IsBounds(cell))
            {
                return int.MinValue;
            }

            return _occupancy[cell.X, cell.Y];
        }

        public Block GetBlock(int blockId)
        {
            if (!_blocks.TryGetValue(blockId, out Block block))
            {
                throw new KeyNotFoundException($"Block not found: {blockId}");
            }

            return block;
        }

        public bool TryMove(int blockId, Direction direction, out int movedSteps)
        {
            Block block = GetBlock(blockId);

            movedSteps = 0;

            if (!CanStep(block, direction))
            {
                return false;
            }

            while (CanStep(block, direction))
            {
                StepOnce(block, direction);

                movedSteps++;
            }
            
            return movedSteps > 0;
        }

        public bool TryHandleExit(Block block, Direction lastMoveDirection)
        {
            if (_exists.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < _exists.Count; i++)
            {
                Exit exit = _exists[i];

                if (SideToDirection(exit.Side) != lastMoveDirection)
                {
                    continue;
                }
                
                if (CanExitThroughGate(block, exit))
                {
                    RemoveBlock(block.Id);
                    
                    BlockExited?.Invoke(block, exit);
                    
                    return true;
                }
            }

            return false;
        }

        private bool CanExitThroughGate(Block block, Exit exit)
        {
            if (exit.ColorId != -1 && block.ColorId != exit.ColorId)
            {
                return false;
            }

            Direction direction = SideToDirection(exit.Side);

            Cell delta = direction.ToDelta();

            List<Cell> edge = GetLeadingEdge(block.Cells, direction);

            bool anyWillLeave = false;

            foreach (Cell cell in edge)
            {
                Cell next = cell + delta;

                if (!IsBounds(next))
                {
                    anyWillLeave = true;

                    if (!IsWithinExitRange(cell, exit))
                    {
                        return false;
                    }
                }
            }

            if (!anyWillLeave)
            {
                return false;
            }

            return true;
        }

        private bool IsWithinExitRange(Cell cell, Exit exit)
        {
            return exit.Side switch
            {
                Side.Left or Side.Right => cell.Y >= exit.From && cell.Y <= exit.To,
                Side.Up or Side.Down => cell.X >= exit.From && cell.X <= exit.To,
                _ => false
            };
        }

        private static Direction SideToDirection(Side side)
        {
            return side switch
            {
                Side.Left => Direction.Left,
                Side.Right => Direction.Right,
                Side.Up => Direction.Up,
                Side.Down => Direction.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public bool TryMoveOneStep(int blockId, Direction direction)
        {
            Block block = GetBlock(blockId);

            if (!CanStep(block, direction))
            {
                return false;
            }

            StepOnce(block, direction);
            
            return true;
        }

        private bool CanStep(Block block, Direction direction)
        {
            Cell delta = direction.ToDelta();

            List<Cell> edge = GetLeadingEdge(block.Cells, direction);

            foreach (Cell cell in edge)
            {
                Cell t = cell + delta;

                if (!IsBounds(t))
                {
                    return false;
                }

                int other = _occupancy[t.X, t.Y];

                if (other != -1 && other != block.Id)
                {
                    return false;
                }
            }

            return true;
        }

        private void StepOnce(Block block, Direction direction)
        {
            Cell delta = direction.ToDelta();

            foreach (Cell cell in block.Cells)
            {
                _occupancy[cell.X, cell.Y] = -1;
            }

            for (int i = 0; i < block.Cells.Count; i++)
            {
                block.Cells[i] = block.Cells[i] + delta;
            }

            foreach (Cell cell in block.Cells)
            {
                _occupancy[cell.X, cell.Y] = block.Id;
            }
            
            BlockMoved?.Invoke(block);
        }

        public List<Cell> GetLeadingEdge(IReadOnlyList<Cell> cells, Direction direction)
        {
            if (cells == null || cells.Count == 0)
            {
                return new List<Cell>();
            }

            if (direction.IsHorizontal())
            {
                Dictionary<int, Cell> bestByRow = new ();

                foreach (Cell cell in cells)
                {
                    int key = cell.Y;

                    if (!bestByRow.TryGetValue(key, out Cell best))
                    {
                        bestByRow[key] = cell;
                        continue;
                    }

                    if (direction == Direction.Right)
                    {
                        if (cell.X > best.X)
                        {
                            bestByRow[key] = cell;
                        }
                    }
                    else
                    {
                        if (cell.X < best.X)
                        {
                            bestByRow[key] = cell;
                        }
                    }
                }

                return bestByRow.Values.ToList();
            }
            else
            {
                Dictionary<int, Cell> bestByColumn = new ();

                foreach (Cell cell in cells)
                {
                    int key = cell.X;

                    if (!bestByColumn.TryGetValue(key, out Cell best))
                    {
                        bestByColumn[key] = cell;
                        continue;
                    }

                    if (direction == Direction.Up)
                    {
                        if (cell.Y > best.Y)
                        {
                            bestByColumn[key] = cell;
                        }
                    }
                    else
                    {
                        if (cell.Y < best.Y)
                        {
                            bestByColumn[key] = cell;
                        }
                    }
                }

                return bestByColumn.Values.ToList();
            }
        }
    }
}
