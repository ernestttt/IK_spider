using IKSpider.Input;
using UnityEngine;

namespace IKSpider.Movement{
    public class SpiderMover : MonoBehaviour
    {
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private float _rotationSpeed = 0.3f;
        [SerializeField] private float _moveSpeed = 2.5f;

        private float _forwardMove;
        private float _rotation;

        private void Update()
        {
            HandleInput();
            transform.position += _forwardMove * Time.deltaTime * transform.forward;

            Quaternion directionRotation =
                    Quaternion.AngleAxis(_rotation * Time.deltaTime, transform.up);
            transform.rotation = directionRotation * transform.rotation;
        }

        private void HandleInput()
        {
            _forwardMove = Mathf.Clamp(_inputManager.ForwardMove * _moveSpeed, 0, float.PositiveInfinity);
            _rotation = _inputManager.Rotation * _rotationSpeed * _moveSpeed * 80;
        }
    }
}

