using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void Update(){
        transform.position = _target.position;
    }
}
