using System.Collections.Generic;
using ColorBlockJam.Core;
using UnityEngine;
using Zenject;

namespace ColorBlockJam.Views
{
    public class ExitView : MonoBehaviour
    {
        [Inject]
        private readonly IInstantiator _instantiator;
        
        [SerializeField]
        private List<EffectView> _effectViewsPrefabs;
        
        [SerializeField]
        private List<CellView> _cellViewPrefabs;

        private readonly List<CellView> _cellViews = new ();
        
        public Exit Exit { get; private set; }
        
        public void Construct(Exit exit, int width, int height)
        {
            Exit = exit;

            float x = 0f;
            float y = 0f;

            Quaternion rotation = Quaternion.identity;
            
            int cellCount = Mathf.Abs(exit.To - exit.From) + 1;

            CellView cellViewPrefab = _cellViewPrefabs.Find(c => c.ColorId == exit.ColorId);
            
            if (exit.Side is Side.Up or Side.Down)
            {
                y = exit.Side == Side.Up ? height - .35f : -.65f;
                
                for (int i = 0; i < cellCount; i++) 
                {
                    x = i + exit.From;

                    rotation = exit.Side == Side.Down ? Quaternion.Euler(Vector3.forward * 180) : Quaternion.identity;
                    
                    CellView cellView = _instantiator.InstantiatePrefabForComponent<CellView>(cellViewPrefab, new Vector3(x, y, 0), rotation, transform);
                    
                    _cellViews.Add(cellView);
                }
            }
            else
            {
                x = exit.Side == Side.Right ? width - .35f : -.65f;
                
                for (int i = 0; i < cellCount; i++) 
                {
                    y = i + exit.From;

                    rotation = Quaternion.Euler(exit.Side == Side.Right
                        ? Vector3.forward * -90f
                        : Vector3.forward * 90);
                        
                    CellView cellView = _instantiator.InstantiatePrefabForComponent<CellView>(cellViewPrefab, new Vector3(x, y, 0), rotation, transform);
                    
                    _cellViews.Add(cellView);
                }
            }
        }

        public void PlayParticleSystem()
        {
            EffectView prefab = _effectViewsPrefabs.Find(e => e.ColorId == Exit.ColorId);
            
            foreach (CellView cellView in _cellViews)
            {
                Vector3 position = cellView.transform.position + (cellView.transform.up * .5f);
                Quaternion rotation = cellView.transform.rotation;
                
                _instantiator.InstantiatePrefabForComponent<ParticleSystem>(prefab, position, rotation, transform);
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}