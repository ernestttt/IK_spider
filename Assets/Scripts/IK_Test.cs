using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Test : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private IK_leg leg;

    private void Update(){
        leg.SetTargetPoint(_target.position);
    }
}
