using System.Collections.Generic;

namespace ColorBlockJam.Core
{
    public sealed class Block
    {
        public int Id { get; }
        public int ColorId { get; }
        
        public List<Cell> Cells { get; } = new ();
     
        public Block(int id, int colorId, IEnumerable<Cell> cells)
        {
            Id = id;
            ColorId = colorId;
            
            Cells.AddRange(cells);
        }

        public override string ToString()
        {
            return $"Block#{Id} (cells:{Cells.Count})";
        }
    }
}