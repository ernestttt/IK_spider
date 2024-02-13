using UnityEngine;

namespace IKSpider.Input
{
    public class InputManager : MonoBehaviour
    {
        private const string vertical = "Vertical";
        private const string mouseX = "Mouse X";

        public float ForwardMove => UnityEngine.Input.GetAxis(vertical);
        public float Rotation => UnityEngine.Input.GetAxis(mouseX);
    }
}

