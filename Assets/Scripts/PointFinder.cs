using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointFinder : MonoBehaviour
{
    [SerializeField] private Mover _mover;
    [Tooltip("points only for one side, they are symetrical")]
    [SerializeField] private Vector3[] _legPlacement;

    private Vector3[] _hitPoints = new Vector3[8];

    private Vector3[] _currentLegPoints = new Vector3[8];

    private Vector3[] CurrentPoints{
        get{
            // fill current leg points
            for(int i = 0; i < _legPlacement.Length; i++){
                _currentLegPoints[i*2] = transform.position + transform.rotation * _legPlacement[i];
                _currentLegPoints[i * 2 + 1] = 
                    transform.position + transform.rotation * 
                        new Vector3(-_legPlacement[i].x, _legPlacement[i].y, _legPlacement[i].z);
            }
            return _currentLegPoints;
        }
    }

    private void Update(){
        DrawLegPlacement();

        for (int i = 0; i < CurrentPoints.Length; i++){
            Ray ray = new Ray(_currentLegPoints[i], -_mover.Normal);

            if(Physics.Raycast(ray, out RaycastHit hit, 4f)){
                _hitPoints[i] = hit.point;
                Debug.DrawLine(_currentLegPoints[i], _hitPoints[i]);
            }
            else if(Physics.SphereCast(ray.origin, 1.5f, ray.direction, out RaycastHit sphereHit, 4f)){
                if(sphereHit.collider is MeshCollider){
                    _hitPoints[i] = sphereHit.point;
                }
                else{
                    _hitPoints[i] = sphereHit.collider.ClosestPoint(_currentLegPoints[i]);
                }
                
                Debug.DrawLine(_currentLegPoints[i], _hitPoints[i]);
            }
            else{
                _hitPoints[i] = Vector3.zero;
            }
        }
    }

    private void DrawLegPlacement(){
        foreach(var leg in CurrentPoints)
        {
            Debug.DrawLine(transform.position, leg);
        }
    }

    private void OnDrawGizmosSelected(){
        foreach(var point in _hitPoints){
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
