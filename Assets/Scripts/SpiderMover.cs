using UnityEngine;

public class SpiderMover : MonoBehaviour
{
    [SerializeField] private float distanceFromFloor = 1.5f;

    private void Update(){
        Ray ray = new Ray(transform.position, transform.InverseTransformDirection(Vector3.down));

        if(Physics.Raycast(ray, out RaycastHit hit, 10)){
            Vector3 diff = transform.position - hit.point;
            float distanceDiff = distanceFromFloor - diff.magnitude;
            if (Mathf.Abs(distanceDiff) > .1f){
                Vector3 addition = diff.normalized * distanceDiff;
                transform.position += addition;
            }
        }
    }
}
