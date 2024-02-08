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
    [SerializeField] private int _numberOfRaycastSteps;

    private Vector3[] _hitPoints = new Vector3[8];
    private Vector3[] _oldPoints = new Vector3[8];
    private bool[] interIndices = new bool[8];
    private float[] _interPolationValues = new float[8];
    private Vector3[] _interPointsWithHeight = new Vector3[8];

    private Vector3[] _currentLegPoints = new Vector3[8];

    public Vector3[] Points => _interPointsWithHeight;

    // find hit poins variables
    private float _tDelta;
    private Vector3[] _linePoints;

    private Vector3[] CurrentPoints{
        // may update this part only once per frame
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

    private void OnValidate(){
        _tDelta = 1.0f / _numberOfRaycastSteps;
        _linePoints = new Vector3[_numberOfRaycastSteps + 1];
    }

    private void Update(){
        DrawLegPlacement();
        FillHitPoints();
        //UpdateOldPointStates();
        //UpdatePoints();
        //UpdateHeight();
    }

    private void UpdateHeight(){
        for(int i = 0; i < _interPointsWithHeight.Length; i++){
            if (true || interIndices[i])
            {
                _interPointsWithHeight[i] = _oldPoints[i]
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
            if (Physics.Linecast(transform.position, CurrentPoints[i], out RaycastHit lineHit)){
                _hitPoints[i] = lineHit.point;
            }
            else
            {
                Vector3 firstPoint = CurrentPoints[i] - _mover.Normal * _mover.DistanceFromSurface * 1.5f;
                Vector3 lastPoint = transform.position - _mover.Normal * _mover.DistanceFromSurface * 1.5f;
                for (int j = 0; j <= _numberOfRaycastSteps; j++)
                {
                    _linePoints[i] = Vector3.Lerp(firstPoint, lastPoint, _tDelta * j);
                }
                for (int k = 0; k < _linePoints.Length; k++){
                    if(Physics.Linecast(CurrentPoints[i], _linePoints[k], out RaycastHit hit)){
                        _hitPoints[i] = hit.point;
                        break;
                    }
                }
            }
        }

        DrawHitLines();
    }

    private void DrawHitLines(){
        for (int i = 0; i < _hitPoints.Length; i++){
            Debug.DrawLine(CurrentPoints[i], _hitPoints[i], Color.green);
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
        DrawPoints(_interPointsWithHeight);
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach(var point in points){
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
