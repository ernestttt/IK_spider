using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IKSpider.IK;

public class LegController : MonoBehaviour
{
    [SerializeField] private PointFinder _pointFinder;
    [SerializeField] private IK_leg[] legs = new IK_leg[8];

    private void Update(){
        for(int i = 0; i < legs.Length; i++){
            legs[i].SetTargetPoint(_pointFinder.Points[i]);
        }
    }
}
