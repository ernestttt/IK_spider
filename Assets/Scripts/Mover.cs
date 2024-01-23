using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float radius = 3f;

    private List<Vector3> _points = new List<Vector3>();

    private void Update(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).
            Where(a => a.transform != transform).ToArray();
        
        _points.Clear();
        foreach (var collider in colliders)
        {
            Vector3 point = collider.ClosestPoint(transform.position);
            _points.Add(point);
            Debug.DrawLine(transform.position,
            point);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);

        foreach(var point in _points)
        {
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
