using ColorBlockJam.Signals;
using UnityEngine;
using Zenject;

namespace ColorBlockJam.Controllers
{
    public class InputController : ITickable
    {
        private readonly SignalCenter _signalCenter;

        public InputController(SignalCenter signalCenter)
        {
            _signalCenter = signalCenter;
        }

        public void Tick()
        {
            if (Input.touchCount == 0)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _signalCenter.Fire(new TouchBeginSignal(touch.position));
            }

            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                _signalCenter.Fire(new TouchDragSignal(touch.position));
            }
            
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _signalCenter.Fire(new TouchEndSignal());
            }
        }
    }
}