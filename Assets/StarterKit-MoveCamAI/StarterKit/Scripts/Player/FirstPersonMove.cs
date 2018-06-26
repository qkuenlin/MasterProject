using UnityEngine;
using System.Collections;

public class FirstPersonMove : MonoBehaviour
{

    // Animation script
    private CharacterAnimation anim;

    // Rotation variables
    private float rotY, rotX;
    public float sensitivity = 10.0f;

    // Speed variables
    public float speed = 10f,
                     speedHalved = 7.5f,
                     speedOrigin = 10f,
                    modifier = 1.0f;

    // Jump!
    private float distToGround;

    private new Rigidbody rigidbody;

    private enum ControlMode { FPS, FREE };

    private ControlMode mode = ControlMode.FPS;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // FixedUpdate is used for physics based movement
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = ControlMode.FPS;
            rigidbody.useGravity = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = ControlMode.FREE;
            rigidbody.useGravity = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) modifier = 5.0f;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) modifier = 1.0f;

        float horizontal = Input.GetAxis("Horizontal"); // set a float to control horizontal input
        float vertical = Input.GetAxis("Vertical"); // set a float to control vertical input
        MouseLook(); // Call the player look function which controls the mouse
        PlayerMove(horizontal, vertical); // Call the move player function sending horizontal and vertical movements
        Jump(); // Call the Jump function! Woot!
    }

    private void MouseLook()
    {
        rotX += Input.GetAxis("Mouse X") * sensitivity; // set a float to control Mouse X input
        rotY += Input.GetAxis("Mouse Y") * sensitivity; // set a float to control Mouse Y input
        rotY = Mathf.Clamp(rotY, -90f, 90); // Lock rotY to a 90 degree angle for looking up and down

        transform.localEulerAngles = new Vector3(0, rotX, 0); // Rotate the player mode left and right
        Camera.main.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
    }

    private void PlayerMove(float h, float v)
    {
        if (h != 0f || v != 0f) // If horizontal or vertical are pressed then continue
        {
            if (h != 0f && v != 0f) // If horizontal AND vertical are pressed then continue
            {
                speed = speedHalved; // Modify the speed to adjust for moving on an angle
            }
            else // If only horizontal OR vertical are pressed individually then continue
            {
                speed = speedOrigin; // Keep speed to it's original value
            }

            switch (mode)
            {
                case ControlMode.FPS:
                    rigidbody.MovePosition(rigidbody.position + (transform.right * h) * speed * modifier * Time.deltaTime); // Move player based on the horizontal input
                    rigidbody.MovePosition(rigidbody.position + (transform.forward * v) * speed * modifier * Time.deltaTime); // Move player based on the vertical input
                    break;
                case ControlMode.FREE:
                    rigidbody.MovePosition(rigidbody.position + (Camera.main.transform.right * h) * speed * modifier * Time.deltaTime); // Move player based on the horizontal input
                    rigidbody.MovePosition(rigidbody.position + (Camera.main.transform.forward * v) * speed * modifier * Time.deltaTime); // Move player based on the vertical input
                    break;
            }
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // If the Space bar is pressed down then continue
        {
            if (IsGrounded()) // If the player is grounded, this calls a boolean, then continue
            {
                rigidbody.velocity += 5f * Vector3.up; // add velocity to the player on vector UP
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.1f); // Do a ray cast to see if the players collider is 0.1 away from the surface of something
    }
}