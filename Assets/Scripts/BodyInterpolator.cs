using IKSpider.Orientation;
using UnityEngine;

namespace IKSpider.Movement
{
    public class BodyInterpolator : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _height = .5f;
        [SerializeField] private float _distanceInterpolationSpeed = 5;

        private void Update(){
            Vector3 posTo = _targetTransform.position + _targetTransform.up * _height;
            float diff = (posTo - transform.position).magnitude;
            if (diff > .1f)
            {
                float t = _distanceInterpolationSpeed * Time.deltaTime / diff;
                transform.position = Vector3.Lerp(transform.position, posTo, t);
            }
        }
    }
}

