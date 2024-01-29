using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float normalInterpolationSpeed = 30f;
    [SerializeField] private float distanceFromSurface = 2;
    [SerializeField] private float distanceInterpolationSpeed = 10f;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float raycastRangeInAngles = 45f;
    [SerializeField] private float raycastStep = 5f;

    [Header("Sphere settings")]
    [SerializeField] private int raycastResolution = 32;
    [SerializeField] private float sphereRadius = 3f;

    private List<Vector3> _hitNormals = new List<Vector3>();
    private List<Vector3> _rayCastPoints = new List<Vector3>();
    private List<Vector3> _raycastDirections = new List<Vector3>();


    private Vector3 _currentNormal = Vector3.up;

    private float _moveInput = 0f;
    private float _rotationInput = 0f;

    private float _rayCastTimeInterval = 0;

    public Vector3 Normal => _currentNormal;


    private void OnValidate() {
        GenerateHemiphereDirections();
    }

    private void Update(){
        FillRaycastNormals();

        HandleInput();
        UpdatePosition();

        // set orientation
        if (_hitNormals.Count > 5)
        {
            Vector3 normal = GetTotalNormal(_hitNormals);
           Debug.DrawLine(transform.position, transform.position + normal * 4, Color.red);
            _currentNormal = GetCurrentNormal(_currentNormal, normal);
            Debug.DrawLine(transform.position, transform.position + _currentNormal * 4, Color.blue);

            // combine normal rotation and input rotation
            Quaternion directionRotation = 
                Quaternion.AngleAxis(_rotationInput * movementSpeed * 80 * Time.deltaTime, transform.up);
            transform.rotation =  directionRotation * transform.rotation;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, _currentNormal);
            transform.rotation = rotation * transform.rotation;
        }
    }

    private void HandleInput(){
        _moveInput = Input.GetAxis("Vertical");
        _rotationInput = Input.GetAxis("Mouse X");
    }
    
    private void UpdatePosition(){
        // for now I'm using two rays two adjust position
        Ray ray = new Ray(transform.position, -_currentNormal);

        Quaternion rot = Quaternion.AngleAxis(-5f, Vector3.right);
        Ray ray1 = new Ray(transform.position + Vector3.up * .2f, rot * -_currentNormal);


        if (!TryToAdjustPos(ray) && _rayCastTimeInterval >= .1f)
        {
            if (TryToAdjustPos(ray1))
            {
                _rayCastTimeInterval = 0;
            }
        }
        else
        {
            _rayCastTimeInterval += Time.deltaTime;
        }

        transform.position += _moveInput * movementSpeed * Time.deltaTime * transform.forward;
    }

    private bool TryToAdjustPos(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit1, distanceFromSurface * 10f))
        {
            Vector3 posTo = hit1.point + _currentNormal * distanceFromSurface;
            float diff = (posTo - transform.position).magnitude;
            if (diff > .1f)
            {
                float t = distanceInterpolationSpeed * Time.deltaTime / diff;
                transform.position = Vector3.Lerp(transform.position, posTo, t);
            }

            return true;
        }
        return false;
    }



    private void FillRaycastNormals(){
        _hitNormals.Clear();
        _rayCastPoints.Clear();

        Vector3 origin = transform.position;

        foreach(Vector3 dir in _raycastDirections){
            //Vector3 resultDir = rotation * dir;
            Ray ray = new Ray(origin, dir);
            var hits = Physics.RaycastAll(ray, sphereRadius * 1.2f);
            if (hits.Length > 0)
            {
                _rayCastPoints.Add(hits[0].point);
                _hitNormals.Add(hits[0].normal);
                foreach(var hit in hits)
                {
                    _rayCastPoints.Add(hit.point);
                    _hitNormals.Add(hit.normal);
                }
            }

            // if(Physics.Raycast(ray, out RaycastHit hit, sphereRadius * 1.2f)){
            //     _rayCastPoints.Add(hit.point);
            //     _hitNormals.Add(hit.normal);
            // }
        }
    }


    private Vector3 GetCurrentNormal(Vector3 from, Vector3 to){
        Quaternion rot1 = Quaternion.FromToRotation(from, to);
    
        float angle = Vector3.Angle(from, to);
        if(angle < 2f)
            return from;

        float t = normalInterpolationSpeed * Time.deltaTime / angle;

        Quaternion currentRot = Quaternion.Slerp(Quaternion.identity, rot1, t);

        return (currentRot * from).normalized;
    }

    private Vector3[] GenerateOctaspherePoints(){
        int totalResolution = raycastResolution * raycastResolution;

        Vector3[] points = new Vector3[totalResolution];

        for (int i = 0; i < points.Length; i++){
            float inverseResolution = 1f / raycastResolution;
            float y = i / raycastResolution * inverseResolution - 0.5f;
            float x = i % raycastResolution * inverseResolution - 0.5f;
            
            float z = 0.5f - Mathf.Abs(x) - Mathf.Abs(y);
            
            float offset = Mathf.Max(-z, 0);
            x += x < 0 ? offset : -offset;
            y += y < 0 ? offset : -offset;
            float scale = 1f / Mathf.Sqrt(x * x + y * y + z * z);

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
        if (Vector3.Angle(_currentNormal, totalNormal) < 0f){
            return _currentNormal;
        }
        return totalNormal.normalized;
    }

    private void GenerateHemiphereDirections(){
        _raycastDirections = GenerateOctaspherePoints().ToList();
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
       //DrawPoints(_rayCastPoints);
       //DrawPoints(_directions.Select(a => transform.position + a * 3));
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
