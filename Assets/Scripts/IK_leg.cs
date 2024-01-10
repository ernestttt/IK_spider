using System;
using UnityEngine;

public class IK_leg : MonoBehaviour
{
    [SerializeField] private Transform _base;
    [SerializeField] private Transform _target;

    [SerializeField] private float[] chainLengths;

    private void Start()
    {
        // Initialization code goes here
    }

    private void Update()
    {
        Vector3 fromBase2Target = _target.position - _base.position;
        float baseLength = fromBase2Target.magnitude;
        float length1 = chainLengths[0];
        float length2 = chainLengths[1];
        float angle1 = FindAngle(baseLength, length1, length2);
        float angle2 = FindAngle(length1, length2, baseLength);

        Vector3 point1 = new Vector3(length1 * Mathf.Cos(angle1), length1 * Mathf.Sin(angle1), 0);
        Vector3 baseVector = Vector3.right * baseLength;

        // Rotate everything by two angles
        Vector3 base2TargetYZero = new Vector3(fromBase2Target.x, 0, fromBase2Target.z);
        Vector3 axis = Vector3.Cross(fromBase2Target, base2TargetYZero);
        float angleX = Vector3.SignedAngle(base2TargetYZero, fromBase2Target, axis);
        Quaternion additionalRot = Quaternion.AngleAxis(angleX, Vector3.forward * Mathf.Sign(Vector3.Dot(Vector3.forward, axis)) * Mathf.Sign(Vector3.Dot(Vector3.right, base2TargetYZero)));

        axis = Vector3.Cross(base2TargetYZero, Vector3.right);
        float angleY = Vector3.SignedAngle(Vector3.right, base2TargetYZero, axis);
        additionalRot = Quaternion.AngleAxis(angleY, Vector3.up * Mathf.Sign(Vector3.Dot(Vector3.up, axis))) * additionalRot;

        point1 = additionalRot * point1;
        baseVector = additionalRot * baseVector;

        Debug.DrawLine(_base.position, _base.position + baseVector);
        Debug.DrawLine(_base.position, _base.position + point1, Color.red);
        Debug.DrawLine(_base.position + point1, _base.position + baseVector, Color.blue);
    }

    private float FindAngle(float a, float b, float c)
    {
        return Mathf.Acos((a * a + b * b - c * c) / (2 * a * b));
    }
}