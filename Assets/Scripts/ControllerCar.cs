using UnityEngine;
using System.Collections;

public class ControllerCar : MonoBehaviour {

    //Se ok sostituire con Player VR
    private MyControllerCar _controller;

    private void Awake()
    {
        _controller = GetComponent<MyControllerCar>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float handbrake = Input.GetAxis("Jump");
        _controller.Move(h, v, v, handbrake);
    }
}
