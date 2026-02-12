using UnityEngine;

namespace ColorBlockJam.Views
{
    public class EffectView : MonoBehaviour
    {
        [SerializeField] 
        private int _colorId;

        public int ColorId => _colorId;
    }
}