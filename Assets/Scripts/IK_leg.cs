using UnityEngine;

namespace IKSpider.IK
{
    public class IK_leg : MonoBehaviour
    {
        [Header("Chain lengths")]
        [SerializeField] private float length1 = .15f;
        [SerializeField] private float length2 = .15f;
        [SerializeField] private float length3 = .15f;

        [Header("Other settings")]
        [SerializeField] private float _thickness = .1f;

        private Vector3[] _points;
        private float[] _lengths;

        private float totalLegLength;

        private Vector3 _targetPoint = Vector3.zero;

        public void SetTargetPoint(Vector3 target)
        {
            _targetPoint = target;
        }

        private void Start()
        {
            _points = new Vector3[4];
            _lengths = new float[] { length1, length2, length3 };

            totalLegLength = length3 + length2 + length1;
        }

        private void Update()
        {
            Vector3 fromBase2Target = _targetPoint - transform.position;

            float baseLength = fromBase2Target.magnitude;
            if (baseLength > totalLegLength)
            {
                fromBase2Target = Vector3.ClampMagnitude(fromBase2Target, totalLegLength);
                baseLength = totalLegLength;
            }

            // this part for even angles between chains
            float lenght1and2 = Mathf.Lerp(length1, length2 + length1, baseLength / totalLegLength);
            float angle2 = FindAngle(baseLength, lenght1and2, length3);
            float angle1 = FindAngle(length1, lenght1and2, length2) + angle2;

            Vector3 point1 = new Vector3(length1 * Mathf.Cos(angle1), length1 * Mathf.Sin(angle1), 0);
            Vector3 point2 = new Vector3(lenght1and2 * Mathf.Cos(angle2), lenght1and2 * Mathf.Sin(angle2), 0);
            
        
            Vector3 cross1 = Vector3.Cross(fromBase2Target, Vector3.up).normalized;


            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, cross1);
            point1 = rot * point1;
            point2 = rot * point2;

            if (Vector3.Angle(Vector3.forward, cross1) == 180){
                Quaternion rotAroundCross1 = Quaternion.AngleAxis(180, Vector3.forward);
                point1 = rotAroundCross1 * point1;
                point2 = rotAroundCross1 * point2;
            }

            Vector3 cross2 = Vector3.Cross(Vector3.up, cross1);
            Quaternion rot1 = Quaternion.FromToRotation(cross2, fromBase2Target);
            point1 = rot1 * point1;
            point2 = rot1 * point2;

            // align all to y axis
            Vector3 cross3 = Vector3.Cross(fromBase2Target, point1);
            Vector3 cross4 = Vector3.Cross(cross3, fromBase2Target);
            Vector3 cross5 = Vector3.Cross(fromBase2Target, transform.up);
            Vector3 cross6 = Vector3.Cross(cross5, fromBase2Target);

            Quaternion rot2 = Quaternion.FromToRotation(cross4, cross6);
            point1 = rot2 * point1;
            point2 = rot2 * point2;

            // fill points, 0 is the base point, 1,2 correspondingly point1, point2, 
            // 3 is the targetPoint or max length
            _points[0] = transform.position;
            _points[1] = transform.position + point1;
            _points[2] = transform.position + point2;
            _points[3] = transform.position + fromBase2Target;

            DrawDebugLines();
            //UpdateObjects();
        }

        private void DrawDebugLines()
        {
            Debug.DrawLine(_points[0], _points[3]);
            Debug.DrawLine(_points[0], _points[1]);
            Debug.DrawLine(_points[1], _points[2]);
            Debug.DrawLine(_points[2], _points[3]);
            //Debug.DrawLine(_points[0], _points[2], Color.red);
        }

        // private void UpdateObjects(){
        //     // adjust positions
        //     Vector3 pos1 = _points[0] + (_points[1] - _points[0]) * .5f;
        //     Vector3 pos2 = _points[1] + (_points[2] - _points[1]) * .5f;
        //     Vector3 pos3 = _points[2] + (_points[3] - _points[2]) * .5f;

        //     _chainObjs[0].position = pos1;
        //     _chainObjs[1].position = pos2;
        //     _chainObjs[2].position = pos3;

        //     // adjust rotation
        //     Vector3 forward1 = (_points[1] - _points[0]);
        //     Vector3 forward2 = (_points[2] - _points[1]);
        //     Vector3 forward3 = (_points[3] - _points[2]);

        //     _chainObjs[0].forward = forward1;
        //     _chainObjs[1].forward = forward2;
        //     _chainObjs[2].forward = forward3;
        // }

        private float FindAngle(float a, float b, float c)
        {
            float cosine = (a * a + b * b - c * c) / (2 * a * b);
            return Mathf.Acos(cosine);
        }
    }
}