using ColorBlockJam.Core;
using UnityEngine;

namespace ColorBlockJam.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] 
        private int _colorId = -1;
        
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField] 
        private SpriteRenderer _highlightObject;

        public int ColorId => _colorId;

        public Cell Cell { get; private set; }
        
        public void Construct(Cell cell)
        {
            Cell = cell;
        }
        
        public void SetHighlightObjectStatus(bool status)
        {
            _highlightObject.gameObject.SetActive(status);
            
            _highlightObject.sortingOrder = status ? 3 : 1;
            _spriteRenderer.sortingOrder = status ? 4 : 2;
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}