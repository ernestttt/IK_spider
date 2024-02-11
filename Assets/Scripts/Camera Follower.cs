using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float speed = 5;
    [SerializeField] private float height = 5;
    [SerializeField] private float width = 10;

    private void Update(){
        Vector3 targetPosition = _target.position + _target.up * height - _target.forward * width;
        float distance = (targetPosition - transform.position).magnitude;
        float t = speed * Time.deltaTime / distance;
        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, t);
        transform.position = newPos;
        
        Vector3 directionVector = (_target.position - transform.position).normalized;
        transform.forward = directionVector;
    }
}
