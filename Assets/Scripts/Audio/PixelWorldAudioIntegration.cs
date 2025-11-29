using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    /// <summary>
    /// Helper component to integrate audio with PixelWorldManager mouse painting.
    /// Attach this to the same GameObject as PixelWorldManager.
    /// </summary>
    [RequireComponent(typeof(PixelWorldManager))]
    public class PixelWorldAudioIntegration : MonoBehaviour
    {
        private bool _wasLeftMousePressed;
        private bool _wasRightMousePressed;
        private bool _wasMiddleMousePressed;

        private void Update()
        {
            if (Mouse.current == null) return;

            bool isLeftPressed = Mouse.current.leftButton.isPressed;
            bool isRightPressed = Mouse.current.rightButton.isPressed;
            bool isMiddlePressed = Mouse.current.middleButton.isPressed;

            // Trigger sounds on mouse button press (not hold)
            if (isLeftPressed && !_wasLeftMousePressed)
            {
                AudioEventTriggers.OnPaintSand();
            }

            if (isRightPressed && !_wasRightMousePressed)
            {
                AudioEventTriggers.OnPaintWater();
            }

            if (isMiddlePressed && !_wasMiddleMousePressed)
            {
                AudioEventTriggers.OnEraseTerrain();
            }

            _wasLeftMousePressed = isLeftPressed;
            _wasRightMousePressed = isRightPressed;
            _wasMiddleMousePressed = isMiddlePressed;
        }
    }
}

