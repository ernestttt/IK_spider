using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float radius = 3f;

    private List<Vector3> _meshPoints = new List<Vector3>();
    private List<Vector3> _closestPoints = new List<Vector3>();
    private List<Vector3> _rayPoints = new List<Vector3>();
    private List<Vector3> _directions = new List<Vector3>();

    [Header("Sphere settings"), SerializeField]
    private int resolution = 32;

    private float squareRadius;

    private void OnValidate() {
        squareRadius = radius * radius;
        GenerateHemiphereDirections();
    }

    private void Update(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).
            Where(a => a.transform != transform).ToArray();

        FillClosestPoints(colliders);
        FillMeshPoints(colliders);
        FillRaycastPoints();
        if(_rayPoints.Count > 0)
        {
            Vector3 normal = GetNormalForBody(transform.position, _rayPoints);
            Debug.DrawLine(transform.position, transform.position + normal * 4, Color.red);
        }
    }

    private void FillRaycastPoints(){
        _rayPoints.Clear();

        Vector3 origin = transform.position;
        Quaternion rotation = transform.rotation;

        foreach(Vector3 dir in _directions){
            Vector3 resultDir = rotation * dir;
            Ray ray = new Ray(origin, resultDir);
            
            if(Physics.Raycast(ray, out RaycastHit hit, radius * 1.1f))
            {
                _rayPoints.Add(hit.point);
            }
        }
    }

    private void FillMeshPoints(IEnumerable<Collider> colliders)
    {
        _meshPoints.Clear();
        foreach (var collider in colliders)
        {
            var filter = collider.GetComponent<MeshFilter>();
            if (filter)
            {
                Mesh mesh = filter.sharedMesh;
                Vector3[] vertices = mesh.vertices;

                foreach (Vector3 vertex in vertices)
                {
                    Vector3 transformedVertex = collider.transform.TransformPoint(vertex);
                    if (squareRadius >= (transform.position - transformedVertex).sqrMagnitude)
                    {
                        Vector3 pointInCircle = collider.transform.TransformPoint(vertex);
                        _meshPoints.Add(pointInCircle);
                    }
                }
            }
        }
    }

    private void FillClosestPoints(IEnumerable<Collider> colliders){
        _closestPoints.Clear();
        foreach (var collider in colliders)
        {
            Vector3 point = collider.ClosestPoint(transform.position);
            _closestPoints.Add(point);
        }
    }

    private Vector3[] GenerateOctaspherePoints(){
        int totalResolution = resolution * resolution;

        Vector3[] points = new Vector3[totalResolution];

        for (int i = 0; i < points.Length; i++){
            float inverseResolution = 1f / resolution;
            float y = (i / resolution) * inverseResolution - 0.5f;
            float x = (i % resolution) * inverseResolution - 0.5f;
            
            float z = 0.5f - Mathf.Abs(x) - Mathf.Abs(y);
            
            float offset = Mathf.Max(-z, 0);
            x += x < 0 ? offset : -offset;
            y += y < 0 ? offset : -offset;
            float scale = (1f / Mathf.Sqrt(x * x + y * y + z * z));

            x *= scale;
            y *= scale;
            z *= scale;
            points[i] = new Vector3(x,y,z);
        }

        return points;
    }

    private void GenerateHemiphereDirections(){
        _directions = GenerateOctaspherePoints().Where(a => a.y <= 0).ToList();
    }

    private Vector3 GetNormalByExtremeValues(Vector3 pos, IEnumerable<Vector3> cloud){
        var orderedByY = cloud.OrderBy(a => Mathf.Max(a.y));
        Vector3 point1 = orderedByY.Last();
        Vector3 point2 = orderedByY.First();
        Vector3 point3 = cloud.OrderBy(a => Mathf.Max(a.x)).Last();

        Vector3 cross = Vector3.Cross(point3 - point1, point2 - point1);
        Vector3 minusCross = -cross;

        Vector3 normal = (new Vector3[] { cross, minusCross }).OrderBy(a => (a - pos).sqrMagnitude).First().normalized;
        Debug.DrawLine(point1, point2);
        Debug.DrawLine(point2, point3);
        Debug.DrawLine(point1, point3);
        return normal;
    }

    private Vector3 GetNormalForBody(Vector3 pos, IEnumerable<Vector3> cloud){
        //Vector3 point1 = cloud.OrderBy(a => (a - pos).sqrMagnitude).Last();
        Vector3 point1 = cloud.OrderBy(a => Mathf.Max(a.y)).First();
        Vector3 point2 = cloud.OrderBy(a => Mathf.Max(a.y)).Last();
        //Vector3 point2 = cloud.OrderBy(a => (a - point1).sqrMagnitude).Last();
        Vector3 middlePoint = point1 + 0.5f * (point2 - point1);
        Vector3 point3 = cloud.
            Where(a => a != point1 && a != point2).
            OrderBy(a => (a - middlePoint).sqrMagnitude).Last();

        Vector3 cross = Vector3.Cross(point3 - point1, point2 - point1);
        Vector3 minusCross = -cross;

        Vector3 normal = (new Vector3[]{cross, minusCross}).
            OrderBy(a => (a - pos).sqrMagnitude).First().normalized;
        Debug.DrawLine(point1, point2);
        Debug.DrawLine(point2, point3);
        Debug.DrawLine(point1, point3);
        return normal;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.green;
        DrawPoints(_closestPoints);
        Gizmos.color = Color.cyan;
        DrawPoints(_meshPoints);
        Gizmos.color = Color.red;
        _rayPoints.AddRange(_meshPoints);
        _rayPoints.AddRange(_closestPoints);
        DrawPoints(_rayPoints);
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
