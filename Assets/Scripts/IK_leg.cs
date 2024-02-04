using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class IK_leg : MonoBehaviour
{
    [Header("Chain lengths")]
    [SerializeField] private float length1 = .15f;
    [SerializeField] private float length2 = .15f;
    [SerializeField] private float length3 = .15f;

    [Header("Other settings")]
    [SerializeField] private float _rotationCoefX = .1f;
    [SerializeField] private float _thickness = .1f;
    [SerializeField] private Transform _chainObject;
    

    private Transform[] _chainObjs;
    private Vector3[] _points;
    private float[] _lengths;

    private float totalLegLength;

    private Vector3 _targetPoint = Vector3.zero;

    public void SetTargetPoint(Vector3 target){
        _targetPoint = target;
    }

    private void Start(){
        _points = new Vector3[4];
        _lengths = new float[] {length1, length2, length3};
        _chainObjs = new Transform[3];

        for(int i = 0; i < _chainObjs.Length; i++){
            _chainObjs[i] = Instantiate(_chainObject, transform);
            _chainObjs[i].localScale =
                 new Vector3(_lengths[i] * _thickness, _lengths[i] * _thickness, _lengths[i]);
        }

        totalLegLength = length3 + length2 + length1;
    }

    private void Update()
    {
        Vector3 fromBase2Target = _targetPoint - transform.position;

        float baseLength = fromBase2Target.magnitude;
        if (baseLength > totalLegLength){
            fromBase2Target = Vector3.ClampMagnitude(fromBase2Target, totalLegLength);
            baseLength = totalLegLength;
        }
        
        // this part for even angles between chains
        float lenght1and2 = Mathf.Lerp(length1, length2+length1, baseLength / totalLegLength);
        float angle1 = FindAngle(baseLength, lenght1and2, length3);
        float angle2 = FindAngle(length1, lenght1and2, length2) + angle1;

        Vector3 point1 = new Vector3(length1 * Mathf.Cos(angle2), length1 * Mathf.Sin(angle2), 0);
        Vector3 point2 = new Vector3(lenght1and2 * Mathf.Cos(angle1), lenght1and2 * Mathf.Sin(angle1), 0);

        Vector3 baseFromTargetYZero = new Vector3(fromBase2Target.x, 0, fromBase2Target.z);
        float baseTargetAngle = Vector3.SignedAngle(transform.right, baseFromTargetYZero, transform.up);
        Quaternion rotBaseTarget =   Quaternion.AngleAxis(baseTargetAngle * _rotationCoefX, fromBase2Target);
        transform.rotation =  rotBaseTarget *transform.parent.rotation;

        // Rotate everything by two angles
        Vector3 base2TargetYZero = new Vector3(fromBase2Target.x, 0, fromBase2Target.z);
        Vector3 axis = Vector3.Cross(fromBase2Target, base2TargetYZero);
        float angleX = Vector3.SignedAngle(base2TargetYZero, fromBase2Target, axis);
        
        Quaternion additionalRot =
            Quaternion.AngleAxis(angleX, 
            transform.forward * Mathf.Sign(Vector3.Dot(transform.forward, axis)) * Mathf.Sign(Vector3.Dot(transform.right, base2TargetYZero)));

        axis = Vector3.Cross(base2TargetYZero, transform.right);
        float angleY = Vector3.SignedAngle(transform.right, base2TargetYZero, axis);
        additionalRot = Quaternion.AngleAxis(angleY, transform.up * Mathf.Sign(Vector3.Dot(transform.up, axis))) * additionalRot;
        additionalRot *= rotBaseTarget;
        point2 =  point2;
        point1 =  point1;

        // fill points, 0 is the base point, 1,2 correspondingly point1, point2, 
        // 3 is the targetPoint or max length
        _points[0] = transform.position;
        _points[1] = transform.position + point1;
        _points[2] = transform.position + point2;
        _points[3] = transform.position + fromBase2Target;

        DrawDebugLines();
        UpdateObjects();
    }

    private void DrawDebugLines(){
        Debug.DrawLine(_points[0], _points[3]);
        Debug.DrawLine(_points[0], _points[1]);
        Debug.DrawLine(_points[1], _points[2]);
        Debug.DrawLine(_points[2], _points[3]);
    }

    private void UpdateObjects(){
        // adjust positions
        Vector3 pos1 = _points[0] + (_points[1] - _points[0]) * .5f;
        Vector3 pos2 = _points[1] + (_points[2] - _points[1]) * .5f;
        Vector3 pos3 = _points[2] + (_points[3] - _points[2]) * .5f;

        _chainObjs[0].position = pos1;
        _chainObjs[1].position = pos2;
        _chainObjs[2].position = pos3;

        // adjust rotation
        Vector3 forward1 = (_points[1] - _points[0]);
        Vector3 forward2 = (_points[2] - _points[1]);
        Vector3 forward3 = (_points[3] - _points[2]);

        _chainObjs[0].forward = forward1;
        _chainObjs[1].forward = forward2;
        _chainObjs[2].forward = forward3;
    }

    private float FindAngle(float a, float b, float c)
    {
        float cosine = (a * a + b * b - c * c) / (2 * a * b);
        return Mathf.Acos(cosine);
    }
}