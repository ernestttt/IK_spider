using IKSpider.Orientation;
using UnityEngine;

namespace IKSpider.Movement{
    public class Sticker : MonoBehaviour
    {
        [SerializeField] private NormalFinder _normalFinder;
        [SerializeField] private float _distanceFromSurface;
        [SerializeField] private float _sphereRadius = 1;
        [SerializeField] private Transform _stickTransform;

        private void Update(){
            Vector3 normal = _normalFinder.GetTotalNormal();

            Ray ray = new Ray(transform.position + normal * 1.5f, -normal);
            // stick body to the surface
            AdjustPos(ray);
        }

        private void AdjustPos(Ray ray)
        {
            Vector3 sphereVector = Vector3.zero;
            if (Physics.SphereCast(ray.origin, _sphereRadius, ray.direction, out RaycastHit sphereHit, 5))
            {
                sphereVector = sphereHit.point - transform.position;
                Debug.DrawLine(transform.position, sphereHit.point, Color.red);
            }

            Debug.DrawLine(ray.origin, ray.origin + ray.direction, Color.blue);

            if (Physics.Raycast(ray, out RaycastHit hit, 5))
            {
                
                Vector3 rayCastVector = hit.point - transform.position;

                // to escape narrow places
                Vector3 lowPoint = GetLowPoint(sphereVector, rayCastVector, hit.point);

                _stickTransform.position = lowPoint - ray.direction * _distanceFromSurface;
                Debug.DrawLine(transform.position, hit.point);
            }
        }

        private Vector3 GetLowPoint(Vector3 sphereVector, Vector3 raycastVector, Vector3 defaultValue){
            Vector3 sphereCastOnRay = Vector3.Project(sphereVector, raycastVector);
            Vector3 lowPoint;
            if (sphereCastOnRay == Vector3.zero)
            {
                lowPoint = defaultValue;
            }
            else
            {
                lowPoint = transform.position + sphereCastOnRay;
            }

            return lowPoint;
        }
    }
}

