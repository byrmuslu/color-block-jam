using UnityEngine;

namespace ColorBlockJam.Views
{
    public class ObstacleView : MonoBehaviour
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}