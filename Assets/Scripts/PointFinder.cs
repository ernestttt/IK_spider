using System.Collections.Generic;
using UnityEngine;
using IKSpider.Movement;

public class PointFinder : MonoBehaviour
{
    [Tooltip("points only for one side, they are symetrical")]
    [SerializeField] private Vector3[] _legPlacement;
    [SerializeField] private float _distanceFromSurface = 2;

    [SerializeField] private float step = 1f;
    [SerializeField] private float lowestStep = .01f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float height = .2f;
    [SerializeField, Range(1,10)] private int _numberOfRaycastSteps;
    [SerializeField] private float _updatePointsInterval = 0.7f;
    [SerializeField] private bool _showDebugLines = false;
    [SerializeField] private float _heightOfRaycast = 3f;

    private Vector3[] _hitPoints = new Vector3[8];
    private Vector3[] _stepPoints = new Vector3[8];
    // for hill
    private Vector3[] _oldPoints = new Vector3[8];
    private Vector3[] _interPoints = new Vector3[8];
    private Vector3[] _interPointsWithHills = new Vector3[8];

    // find hit poins variables
    private float _tDelta;
    private Vector3[] _linePoints;

    // for zigzag pattern
    private bool _zigzagFlag;
    private int[] _firstZigzagIndices = new int[] { 0, 3, 4, 7 };
    private int[] _secondZigzagIndices = new int[] { 1, 2, 5, 6 };
    private float _nextPointUpdateTime = float.MinValue;

    private float TotalSpeed => speed;

    private Vector3[] _currentLegPoints = new Vector3[8];
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

    public Vector3[] Points => _interPointsWithHills;

    private void OnValidate(){
        _tDelta = 1.0f / _numberOfRaycastSteps;
        _linePoints = new Vector3[_numberOfRaycastSteps + 1];
    }

    private void Update(){
        DrawLegPlacement();
        FillHitPoints();
        UpdateOldPointStates();
        UpdateInterPoints();
    }

    private void UpdateInterPoints(){
        for(int i = 0; i < _interPoints.Length; i++){

            float distance = (_stepPoints[i] - _interPoints[i]).magnitude;

            if(distance > 6){
                _interPoints[i] = _stepPoints[i];
                _oldPoints[i] = _stepPoints[i];
                continue;
            }

            float frameT = Mathf.Clamp01(TotalSpeed * Time.deltaTime / distance);
            _interPoints[i] = Vector3.Lerp(_interPoints[i], _stepPoints[i], frameT);

            // get hill
            float distanceStepOld = (_stepPoints[i] - _oldPoints[i]).magnitude;
            float hillT = 0;
            if (distanceStepOld > 0.1f)
            {
                hillT = distance / distanceStepOld;
            }
            
            float hill = GetHill(hillT);
            _interPointsWithHills[i] = _interPoints[i] + transform.up * hill * height;
        }
    }

    private float GetHill(float t){
        return Mathf.Sin(t * Mathf.PI);
    }

    private void UpdateOldPointStates(){
        if(_nextPointUpdateTime < Time.time){
            _nextPointUpdateTime = Time.time + _updatePointsInterval;
            int[] indicesArr = _zigzagFlag ? _firstZigzagIndices : _secondZigzagIndices;
            _zigzagFlag = !_zigzagFlag;

            for (int i = 0; i < indicesArr.Length; i++)
            {
                float distance = (_stepPoints[indicesArr[i]] - _hitPoints[indicesArr[i]]).magnitude;
                if (distance > step)
                {
                    _oldPoints[indicesArr[i]] = _stepPoints[indicesArr[i]];
                    _stepPoints[indicesArr[i]] = _hitPoints[indicesArr[i]];
                }
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
                Vector3 firstPoint = CurrentPoints[i] + (transform.up * height) - transform.up * _distanceFromSurface * 2f;
                Vector3 lastPoint = transform.position - transform.up * _distanceFromSurface * 1.5f;
                for (int j = 0; j <= _numberOfRaycastSteps; j++)
                {
                    _linePoints[j] = Vector3.Lerp(firstPoint, lastPoint, _tDelta * j);
                }
                for (int k = 0; k <=_numberOfRaycastSteps; k++){
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
        if (!_showDebugLines) return;
        for (int i = 0; i < _hitPoints.Length; i++){
            Debug.DrawLine(CurrentPoints[i], _hitPoints[i], Color.green);
        }
    }

    private void DrawLegPlacement(){
        if(!_showDebugLines)    return;
        foreach(var leg in CurrentPoints)
        {
            Debug.DrawLine(transform.position, leg);
        }
    }

    private void OnDrawGizmosSelected(){
        //Gizmos.color = Color.green;
        //DrawPoints(_hitPoints);
        Gizmos.color = Color.blue;
        DrawPoints(_stepPoints);
        Gizmos.color = new Color(1, .6f, 0);
        DrawPoints(_interPointsWithHills);
    }

    private void DrawPoints(IEnumerable<Vector3> points){
        foreach(var point in points){
            Gizmos.DrawSphere(point, .1f);
        }
    }
}
