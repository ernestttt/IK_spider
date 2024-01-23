using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float radius = 3f;

    private List<Vector3> _meshPoints = new List<Vector3>();
    private List<Vector3> _closestPoints = new List<Vector3>();
    

    private float squareRadius;

    private void OnValidate() {
        squareRadius = radius * radius;
    }

    private void Update(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).
            Where(a => a.transform != transform).ToArray();

        _closestPoints.Clear();
        foreach (var collider in colliders)
        {
            Vector3 point = collider.ClosestPoint(transform.position);
            _closestPoints.Add(point);
        }

        _meshPoints.Clear();
        foreach (var collider in colliders){
            var filter = collider.GetComponent<MeshFilter>();
            if(filter){
                Mesh mesh = filter.sharedMesh;
                Vector3[] vertices = mesh.vertices;

                //Vector3 ourPosInColliderSpace = collider.transform.InverseTransformPoint(transform.position);

                foreach(Vector3 vertex in vertices){
                    Vector3 transformedVertex = collider.transform.TransformPoint(vertex);
                    if (squareRadius >= (transform.position - transformedVertex).sqrMagnitude){
                        Vector3 pointInCircle = collider.transform.TransformPoint(vertex);
                        _meshPoints.Add(pointInCircle);
                    }
                }  
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.green;
        DrawPoints(_closestPoints);
        Gizmos.color = Color.cyan;
        DrawPoints(_meshPoints);

    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
