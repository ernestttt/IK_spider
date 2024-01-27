using UnityEngine;
using UnityEngine.Rendering;

public class InputMover : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void Update(){
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 delta = transform.TransformVector(speed * new Vector3(0, 0, vertical * speed * Time.deltaTime));
        transform.position += delta;

        if(true || vertical > .1f)
        {
            float horizontal = Input.GetAxis("Mouse X");
            Debug.Log($"horizontal is {horizontal}");
            if(Mathf.Abs(horizontal) > 0.2f){
                float angle = horizontal * speed * Time.deltaTime * 80f;

                Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
                transform.rotation *= rotation;
            }
            
        }
        
    }
}
