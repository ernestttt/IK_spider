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
        [SerializeField] private bool _showNormal;

        private Vector3 _currentNormal = Vector3.up;

        private float _moveInput = 0f;
        private float _rotationInput = 0f;

        public Vector3 Normal => _currentNormal;
        public float DistanceFromSurface => _distanceFromSurface;
        public float Speed => _movementSpeed;

        private void Update()
        {
            HandleInput();
            UpdatePosition();

            // set orientation
            Vector3 normal = _normalFinder.GetTotalNormal();
            _currentNormal = GetCurrentNormal(_currentNormal, normal);
            ShowDebugNormals(normal, _currentNormal);

            // combine normal rotation and input rotation
            Quaternion directionRotation =
                Quaternion.AngleAxis(_rotationInput * _movementSpeed * 80 * Time.deltaTime, transform.up);
            transform.rotation = directionRotation * transform.rotation;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, _currentNormal);
            transform.rotation = rotation * transform.rotation;
        }

        private void ShowDebugNormals(Vector3 targetNormal, Vector3 currentNormal){
            if(!_showNormal)    return;
            Debug.DrawLine(transform.position, transform.position + targetNormal * 4, Color.red);
            Debug.DrawLine(transform.position, transform.position + currentNormal * 4, Color.blue);
        }

        private void HandleInput()
        {
            _moveInput = Input.GetAxis("Vertical");
            if(_moveInput < 0)
            {
                _moveInput = 0;
            }
                
            _rotationInput = Input.GetAxis("Mouse X")*0.3f;
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
            Vector3 sphereVector = Vector3.zero;
            if(Physics.SphereCast(ray.origin + _currentNormal, 1f, ray.direction, out RaycastHit sphereHit, _distanceFromSurface * 10f))
            {
                sphereVector = sphereHit.point - transform.position;
                Debug.DrawLine(transform.position, sphereHit.point, Color.red);
            }

            if (Physics.Raycast(ray, out RaycastHit hit, _distanceFromSurface * 10f))
            {
                Vector3 rayCastVector = hit.point - transform.position;
                
                Vector3 sphereCastOnRay = Vector3.Project(sphereVector, rayCastVector);
                Vector3 lowPoint;
                if(sphereCastOnRay == Vector3.zero){
                    lowPoint = hit.point;
                }
                else{
                    lowPoint = transform.position + sphereCastOnRay;
                }

                Vector3 posTo = lowPoint + _currentNormal * _distanceFromSurface;
                Debug.DrawLine(transform.position, hit.point);
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

        private Vector3 ProjectVOnU(Vector3 v, Vector3 u){
            if((u == Vector3.zero) || (v == Vector3.zero)){
                return Vector3.zero;
            }

            return (Vector3.Dot(v, u)/Vector3.Dot(u, u)) * u;
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