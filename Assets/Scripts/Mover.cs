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
    private List<Vector3> _hitNormals = new List<Vector3>();
    private List<Vector3> _directions = new List<Vector3>();

    [SerializeField] private float interpolationSpeed = 30f;

    [Header("Sphere settings"), SerializeField]
    private int resolution = 32;

    private float squareRadius;

    private Vector3 currentNormal = Vector3.up;

    [SerializeField] private float distanceFromSurface = 2;
    [SerializeField] private float radiusForDistance = 1.5f;
    [SerializeField] private float distanceInterpolationSpeed = 10f;

    private void OnValidate() {
        squareRadius = radius * radius;
        GenerateHemiphereDirections();
    }

    private void Start(){
        //distanceFromSurface = radius * .5f;
    }

    private void FixedUpdate(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).
            Where(a => a.transform != transform).ToArray();

        FillClosestPoints(colliders);
        FillMeshPoints(colliders);
        FillRaycastPoints();

        // set orientation
        if(_rayPoints.Count > 5)
        {
            Vector3 normal = GetTotalNormal(_hitNormals);
            Debug.DrawLine(transform.position, transform.position + normal * 4, Color.red);
            currentNormal = GetCurrentNormal(currentNormal, normal);
            Debug.DrawLine(transform.position, transform.position + currentNormal * 4, Color.blue);

            transform.up = currentNormal;
            
        }
        UpdatePosition(currentNormal);

    }

    private float timeInterval = 0;
    private void UpdatePosition(Vector3 normal){
        Ray ray = new Ray(transform.position, -normal);
        Quaternion rot = Quaternion.AngleAxis(-5f, Vector3.right);
        Ray ray1 = new Ray(transform.position + Vector3.up * .2f, rot * -normal);

        if (Physics.Raycast(ray, out RaycastHit hit, distanceFromSurface * 2f))
        {
            Debug.DrawLine(transform.position, hit.point);
            Vector3 posTo = hit.point + normal * distanceFromSurface;
            float diff = (posTo - transform.position).magnitude;
            if (diff > .1f)
            {
                float t = distanceInterpolationSpeed * Time.fixedDeltaTime / diff;
                transform.position = Vector3.Lerp(transform.position, posTo, t);

            }
        }
        else if  (timeInterval >= .05f && 
                Physics.Raycast(ray1, out RaycastHit hit1, distanceFromSurface * 2.5f))
        {
            Debug.DrawLine(transform.position, hit1.point);
            Vector3 posTo = hit1.point + normal * distanceFromSurface;
            float diff = (posTo - transform.position).magnitude;
            if (diff > .1f)
            {
                float t = distanceInterpolationSpeed * Time.fixedDeltaTime / diff;
                transform.position = Vector3.Lerp(transform.position, posTo, t);

            }
            timeInterval = 0;
        }
        else{
            timeInterval += Time.fixedDeltaTime;
        }
    }

    private void UpdatePosition(IEnumerable<Vector3> cloud){
        var invertedPoints = cloud.Select(a => transform.InverseTransformPoint(a));
        var orderByX = invertedPoints.OrderBy(a => a.x);
        var orderByY = invertedPoints.OrderBy(a => a.y);
        var orderByZ = invertedPoints.OrderBy(a => a.z);

        Vector3 point1X = orderByX.First();
        Vector3 point2X = orderByX.Last();
        Vector3 point1Y = orderByY.First();
        Vector3 point2Y = orderByY.Last();
        Vector3 point1Z = orderByZ.First();
        Vector3 point2Z = orderByZ.Last();

        Vector3 middlePointX = point1X + (point2X - point1X) * 0.5f;
        Vector3 middlePointY = point1Y + (point2Y - point1Y) * 0.5f;
        Vector3 middlePointZ = point1Z + (point2Z - point1Z) * 0.5f;

        float averageX = (middlePointX.x + middlePointY.x + middlePointZ.x) / 3;
        float averageY = (middlePointX.y + middlePointY.y + middlePointZ.y) / 3;
        float averageZ = (middlePointX.z + middlePointY.z + middlePointZ.z) / 3;

        Vector3 resultPoint = new Vector3(averageX, averageY, averageZ);

        //Vector3 middlePoint1 = middlePointX + (middlePointY - middlePointX) * 0.5f;
       // Vector3 middlePoint2 = middlePointY + (middlePointZ - middlePointY) * 0.5f;

        // Vector3 resultPoint = middlePoint1 + (middlePoint2 - middlePoint1) * 0.5f;
        resultPoint = transform.TransformPoint(resultPoint);
        Vector3 posTo = resultPoint + currentNormal * distanceFromSurface;

        

        float diff = (posTo - transform.position).magnitude;
        if (diff > .3f)
        {
            float t = 15 * Time.deltaTime / diff;
            transform.position = Vector3.Lerp(transform.position, posTo, t);
        }
    }

    private void FillRaycastPoints(){
        _rayPoints.Clear();
        _hitNormals.Clear();

        Vector3 origin = transform.position;
        Quaternion rotation = transform.rotation;

        foreach(Vector3 dir in _directions){
            //Vector3 resultDir = rotation * dir;
            Ray ray = new Ray(origin, dir);
            // var hits = Physics.RaycastAll(ray, radius * 1.2f);
            // if (hits.Length > 0)
            // {
                // _rayPoints.Add(hits[0].point);
                // _hitNormals.Add(hits[0].normal);
                // foreach(var hit in hits)
                // {
                //     _rayPoints.Add(hit.point);
                //     _hitNormals.Add(hit.normal);
                // }
            // }

            if(Physics.Raycast(ray, out RaycastHit hit, radius * 1.2f)){
                _rayPoints.Add(hit.point);
                _hitNormals.Add(hit.normal);
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

    private Vector3 GetCurrentNormal(Vector3 from, Vector3 to){
        //return (from + to).normalized;
        Quaternion rot1 = Quaternion.FromToRotation(from, to);
    

        float angle = Vector3.Angle(from, to);
        Debug.Log(angle);
        if(angle < 1f)
            return from;

        float t = interpolationSpeed * Time.fixedDeltaTime / angle;

        Quaternion currentRot = Quaternion.Slerp(Quaternion.identity, rot1, t);

        return (currentRot * from).normalized;
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

    private Vector3 GetTotalNormal(IEnumerable<Vector3> normals)
    {
        Vector3 totalNormal = Vector3.zero;

        foreach(var normal in normals){
            totalNormal += normal;
        }
        totalNormal = totalNormal.normalized;
        if (Vector3.Angle(currentNormal, totalNormal) < 0f){
            return currentNormal;
        }
        //Debug.DrawLine(transform.position, transform.position + totalNormal.normalized * 5, Color.green);
        return totalNormal.normalized;
    }

    private void GenerateHemiphereDirections(){
        _directions = GenerateOctaspherePoints().ToList();
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

    private Vector3 GetNormalForBody(IEnumerable<Vector3> cloud){
        var inversedPoints = cloud.Select(a => transform.InverseTransformPoint(a));

        // z
        var orderedByZ = inversedPoints.OrderBy(a => a.z);
        Vector3 pointZ1 = orderedByZ.First();
        Vector3 pointZ2 = orderedByZ.Last();
        Vector3 pointZ1transformed = transform.TransformPoint(pointZ1);
        Vector3 pointZ2transformed = transform.TransformPoint(pointZ2);
        Debug.DrawLine(pointZ1transformed, pointZ2transformed, Color.blue);

        // x
        var orderedByX = inversedPoints.OrderBy(a => a.x);
        Vector3 pointX1 = orderedByX.First();
        Vector3 pointX2 = orderedByX.Last();
        Vector3 pointX1transformed = transform.TransformPoint(pointX1);
        Vector3 pointX2transformed = transform.TransformPoint(pointX2);
        Debug.DrawLine(pointX1transformed, pointX2transformed, Color.red);

        Vector3 firstVector = pointX1 - pointZ2;
        Vector3 secondVector = pointX2 - pointZ2;

        Vector3 cross = Vector3.Cross(secondVector, firstVector);
        Vector3 normal = transform.TransformVector(cross).normalized;

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
            OrderBy(a => {
                float squareMagnitude1 = (point1 - a).sqrMagnitude;
                float squareMagnitude2 = (point2 - a).sqrMagnitude;
                float squareMagnitude3 = (middlePoint - a).sqrMagnitude;
                return squareMagnitude1 + squareMagnitude2 + squareMagnitude3;
            }).Last();

        Vector3 cross = Vector3.Cross(point3 - point1, point2 - point1);
        Vector3 minusCross = -cross;

        Vector3 normal = (new Vector3[]{cross, minusCross}).
            OrderBy(a => (a - pos).sqrMagnitude).First().normalized;
        // Debug.DrawLine(point1, point2);
        // Debug.DrawLine(point2, point3);
        // Debug.DrawLine(point1, point3);
        return normal;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.green;
        //DrawPoints(_closestPoints);
        Gizmos.color = Color.cyan;
       // DrawPoints(_meshPoints);
        Gizmos.color = Color.red;
       // _rayPoints.AddRange(_meshPoints);
        //_rayPoints.AddRange(_closestPoints);
        DrawPoints(_rayPoints);
       //DrawPoints(_directions.Select(a => transform.position + a * 3));
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
