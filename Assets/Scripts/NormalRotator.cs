using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKSpider.Orientation
{
    public class NormalRotator : MonoBehaviour
    {
        [SerializeField] private NormalFinder _normalFinder;
        [SerializeField] private Transform _targetTransform;

        private void Update(){
            Quaternion rotation = Quaternion.FromToRotation(_targetTransform.up, _normalFinder.GetTotalNormal());
            _targetTransform.rotation = rotation * _targetTransform.rotation;
        }
    }
}

