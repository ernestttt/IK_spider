using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float radius = 3f;

    private List<Vector3> _points = new List<Vector3>();
    private float squareRadius;

    private void OnValidate() {
        squareRadius = radius * radius;
    }

    private void Update(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).
            Where(a => a.transform != transform).ToArray();
        
        _points.Clear();
        foreach (var collider in colliders)
        {
            Vector3 point = collider.ClosestPoint(transform.position);
            _points.Add(point);
        }

        foreach(var collider in colliders){
            var filter = collider.GetComponent<MeshFilter>();
            if(filter){
                Mesh mesh = filter.sharedMesh;
                Vector3[] vertices = mesh.vertices;

                //Vector3 ourPosInColliderSpace = collider.transform.InverseTransformPoint(transform.position);

                foreach(Vector3 vertex in vertices){
                    Vector3 transformedVertex = collider.transform.TransformPoint(vertex);
                    if (squareRadius >= (transform.position - transformedVertex).sqrMagnitude){
                        Vector3 pointInCircle = collider.transform.TransformPoint(vertex);
                        _points.Add(pointInCircle);
                    }
                }  
            }
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
