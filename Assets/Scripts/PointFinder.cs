using System.Collections.Generic;
using UnityEngine;
using IKSpider.Movement;

public class PointFinder : MonoBehaviour
{
    [SerializeField] private BodyMover _mover;
    [Tooltip("points only for one side, they are symetrical")]
    [SerializeField] private Vector3[] _legPlacement;

    [SerializeField] private float step = 1f;
    [SerializeField] private float lowestStep = .01f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float height = .2f;

    private Vector3[] _hitPoints = new Vector3[8];
    private Vector3[] _oldPoints = new Vector3[8];
    private bool[] interIndices = new bool[8];
    private float[] _interPolationValues = new float[8];
    private Vector3[] _interPoinsWithHeight = new Vector3[8];

    private Vector3[] _currentLegPoints = new Vector3[8];

    public Vector3[] Points => _interPoinsWithHeight;

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
        //DrawLegPlacement();
        FillHitPoints();
        UpdateOldPointStates();
        UpdatePoints();
        UpdateHeight();
    }

    private void UpdateHeight(){
        for(int i = 0; i < _interPoinsWithHeight.Length; i++){
            if (true || interIndices[i])
            {
                _interPoinsWithHeight[i] = _oldPoints[i]
                   + Mathf.Sin(_interPolationValues[i] * Mathf.PI) * _mover.Normal * height;
            }
        }
    }

    private void UpdatePoints(){
        for(int i = 0; i < interIndices.Length; i++){
            if(interIndices[i]){
                Vector3 startPoint = _hitPoints[i];
                Vector3 endPoint = _oldPoints[i];

                float diff = (startPoint - endPoint).magnitude;
                float currentSpeed = speed * Time.deltaTime;

                float t = currentSpeed/diff;
                t = Mathf.Clamp01(t);
                Vector3 interVector = Vector3.Lerp(endPoint, startPoint, t);
                _interPolationValues[i] = t;
               // interVector += Mathf.Sin(t * Mathf.PI) * _mover.Normal * height;
                _oldPoints[i] = interVector;
            }
        }
    }

    private void UpdateOldPointStates(){
        for(int i = 0; i < _oldPoints.Length; i++){
            float diff = (_oldPoints[i] - _hitPoints[i]).magnitude;
            if (!interIndices[i] && diff >= step){
                interIndices[i] = true;
                _interPolationValues[i] =0;
            }
            else if(interIndices[i] && _interPolationValues[i] > .9f){
                _oldPoints[i] = _hitPoints[i];
                interIndices[i] = false;
                _interPolationValues[i] = 1;
            }
            else if(diff > 10f){
                _oldPoints[i] = _hitPoints[i];
            }
        }
    }

    private void FillHitPoints(){
        for (int i = 0; i < CurrentPoints.Length; i++)
        {
            Ray ray = new Ray(CurrentPoints[i], -_mover.Normal);

            Vector3 cross = Vector3.Cross(CurrentPoints[i] - transform.position, _mover.Normal);
            Quaternion rotation = Quaternion.AngleAxis(-10f, cross);
            Ray ray2 = new Ray(CurrentPoints[i], rotation * -_mover.Normal);


            if (Physics.Linecast(CurrentPoints[i], transform.position, out RaycastHit lineHit)){
                _hitPoints[i] = lineHit.point;
                //Debug.DrawLine(CurrentPoints[i], _hitPoints[i], Color.blue);
            }
            else if (Physics.Raycast(ray, out RaycastHit hit, 4f))
            {
                _hitPoints[i] = hit.point;
                //Debug.DrawLine(CurrentPoints[i], _hitPoints[i]);
            }
            else if (Physics.Raycast(ray2, out RaycastHit hit2, 4f) && 
                !Physics.Linecast(hit2.point, CurrentPoints[i]))
            {
                _hitPoints[i] = hit2.point;
               // Debug.DrawLine(CurrentPoints[i], _hitPoints[i], Color.green);
            }
            else if (Physics.SphereCast(ray2.origin, 1.5f, ray2.direction, out RaycastHit sphereHit, 4f))
            {
                if (sphereHit.collider is MeshCollider)
                {
                    _hitPoints[i] = sphereHit.point;
                }
                else
                {
                    Vector3 closestPoint = sphereHit.collider.ClosestPoint(CurrentPoints[i]);
                    if (sphereHit.collider.bounds.Contains(closestPoint)
                        || sphereHit.collider.bounds.Contains(CurrentPoints[i]))
                    {
                        //Debug.Log("Inside collider");
                        //Debug.DrawLine(CurrentPoints[i], _hitPoints[i], Color.red);
                        _hitPoints[i] = closestPoint;
                    }
                    else
                    {
                        _hitPoints[i] = closestPoint;
                    }
                }

                //Debug.DrawLine(CurrentPoints[i], _hitPoints[i]);
            }
            else
            {
                // _hitPoints[i] = Vector3.zero;
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
        return;
        Gizmos.color = Color.green;
        DrawPoints(_hitPoints);
        Gizmos.color = Color.blue;
        DrawPoints(_interPoinsWithHeight);
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach(var point in points){
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
