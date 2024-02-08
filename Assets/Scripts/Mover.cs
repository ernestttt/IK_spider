using UnityEngine;
using IKSpider.Orientation;

namespace IKSpider.Movement
{
    public class BodyMover : MonoBehaviour
    {
        [SerializeField] private float _normalInterpolationSpeed = 30f;
        [SerializeField] private float _distanceFromSurface = 2;
        [SerializeField] private float _distanceInterpolationSpeed = 10f;
        [SerializeField] private float _movementSpeed = 1f;
        [SerializeField] private NormalFinder _normalFinder;

        private Vector3 _currentNormal = Vector3.up;

        private float _moveInput = 0f;
        private float _rotationInput = 0f;

        public Vector3 Normal => _currentNormal;
        public float DistanceFromSurface => _distanceFromSurface;

        private void Update()
        {
            HandleInput();
            UpdatePosition();

            // set orientation
            Vector3 normal = _normalFinder.GetTotalNormal();
            Debug.DrawLine(transform.position, transform.position + normal * 4, Color.red);
            _currentNormal = GetCurrentNormal(_currentNormal, normal);
            Debug.DrawLine(transform.position, transform.position + _currentNormal * 4, Color.blue);

            // combine normal rotation and input rotation
            Quaternion directionRotation =
                Quaternion.AngleAxis(_rotationInput * _movementSpeed * 80 * Time.deltaTime, transform.up);
            transform.rotation = directionRotation * transform.rotation;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, _currentNormal);
            transform.rotation = rotation * transform.rotation;
        }

        private void HandleInput()
        {
            _moveInput = Input.GetAxis("Vertical");
            _rotationInput = Input.GetAxis("Mouse X");
        }

        private void UpdatePosition()
        {
            Ray ray = new Ray(transform.position, -_currentNormal);
            // stick body to the surface
            TryToAdjustPos(ray);

            // update with input
            transform.position += _moveInput * _movementSpeed * Time.deltaTime * transform.forward;
        }

        private bool TryToAdjustPos(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, _distanceFromSurface * 10f))
            {
                Vector3 posTo = hit.point + _currentNormal * _distanceFromSurface;
                float diff = (posTo - transform.position).magnitude;
                if (diff > .1f)
                {
                    float t = _distanceInterpolationSpeed * Time.deltaTime / diff;
                    transform.position = Vector3.Lerp(transform.position, posTo, t);
                }

                return true;
            }
            return false;
        }


        private Vector3 GetCurrentNormal(Vector3 from, Vector3 to)
        {
            Quaternion rot = Quaternion.FromToRotation(from, to);

            float angle = Vector3.Angle(from, to);

            float t = _normalInterpolationSpeed * Time.deltaTime / angle;

            Quaternion currentRot = Quaternion.Slerp(Quaternion.identity, rot, t);

            return (currentRot * from).normalized;
        }
    }
}