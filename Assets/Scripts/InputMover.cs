using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMover : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void Update(){
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Debug.Log($"{horizontal} {vertical}");    
        transform.Translate(0, 0, horizontal * speed);
        transform.Rotate(0, vertical * speed, 0);
    }
}
