using UnityEngine;

public class SpiderMover : MonoBehaviour
{
    [SerializeField] private float distanceFromFloor = 1.5f;

    private void Update(){
        Ray ray = new Ray(transform.position, Vector3.down);

        if(Physics.Raycast(ray, out RaycastHit hit, 10)){
            transform.up = hit.normal;
            if(Mathf.Abs(Vector3.Distance(transform.position, hit.point) - distanceFromFloor) > 0.05f)
            {
                transform.position = hit.point + hit.normal * distanceFromFloor;
            }
            Debug.DrawLine(hit.point, transform.position);
        }
    }
}
