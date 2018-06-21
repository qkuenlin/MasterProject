using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour {

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15f;
    public float sensitivityY = 15f;

    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationY = 0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        HandleRotation();
        HandleMovement();

    }

    void HandleMovement()
    {

    }

    void HandleRotation()
    {
        switch (axes)
        {
            case RotationAxes.MouseX:
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
                break;
            case RotationAxes.MouseY:
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
                break;
            case RotationAxes.MouseXAndY:
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
                break;
        }
    }
}
