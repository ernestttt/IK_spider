using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKSpider.Orientation{
    public class BodyOrientation : MonoBehaviour
    {
        [SerializeField] private float _interpolationSpeed = 90f;
        [SerializeField] private Transform _target;

        private void Update(){
            // z axis interpolate
            transform.rotation = GetInterRotation(transform.forward, _target.forward) * transform.rotation;
            // y axis interpolate
            transform.rotation = GetInterRotation(transform.up, _target.up) * transform.rotation;
        }

        private Quaternion GetInterRotation(Vector3 from, Vector3 to){
            Quaternion rot = Quaternion.FromToRotation(from, to);
            float angle = Vector3.Angle(from, to);
            float t = _interpolationSpeed * Time.deltaTime / angle;
            return Quaternion.Slerp(Quaternion.identity, rot, t);
        }
    }
}

