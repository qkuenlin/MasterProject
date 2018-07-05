using UnityEngine;
using System.Collections;

public class FirstPersonMove : MonoBehaviour
{
    public UI ui;
    public Transform sun;

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

        rotX = transform.localEulerAngles.y;
        rotY = Camera.main.transform.localEulerAngles.x;

    }

    // FixedUpdate is used for physics based movement
    void FixedUpdate()
    {
        if (Input.GetAxis("Cancel") > 0) Application.Quit();

        if (Input.GetKeyDown(KeyCode.F1)) ui.ShowUI = !ui.ShowUI;

        float controlMode = Input.GetAxis("Control Mode");
        if (controlMode > 0)
        {
            mode = ControlMode.FPS;
            rigidbody.useGravity = true;
        }
        else if (controlMode < 0)
        {
            mode = ControlMode.FREE;
            rigidbody.useGravity = false;
        }

        modifier = Mathf.Lerp(1.0f, 5.0f, Input.GetAxis("Accelerate"));

        float sunX = Input.GetAxis("SunX");
        float sunY = Input.GetAxis("SunY");
        SunRotation(sunX, sunY);

        float horizontal = Input.GetAxis("Horizontal"); // set a float to control horizontal input
        float vertical = Input.GetAxis("Vertical"); // set a float to control vertical input
        float height = Input.GetAxis("Up_Down");
        PlayerMove(horizontal, vertical, height); // Call the move player function sending horizontal and vertical movements

        MouseLook(); // Call the player look function which controls the mouse
    }

    private void SunRotation(float x, float y)
    {
        sun.localEulerAngles += new Vector3(x, y, 0);
    }

    private void MouseLook()
    {
        rotX += Input.GetAxis("Mouse X") * sensitivity; // set a float to control Mouse X input
        rotY += Input.GetAxis("Mouse Y") * sensitivity; // set a float to control Mouse Y input
        rotY = Mathf.Clamp(rotY, -90f, 90); // Lock rotY to a 90 degree angle for looking up and down

        transform.localEulerAngles = new Vector3(0, rotX, 0); // Rotate the player mode left and right
        Camera.main.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
    }

    private void PlayerMove(float h, float v, float ud)
    {
        if (h != 0f || v != 0f || ud != 0f) // If horizontal or vertical are pressed then continue
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
                    if(ud > 0)
                    {
                        if (IsGrounded()) // If the player is grounded, this calls a boolean, then continue
                        {
                            rigidbody.velocity += 5f * Vector3.up; // add velocity to the player on vector UP
                        }
                    }
                    rigidbody.MovePosition(rigidbody.position + (transform.right * h) * speed * modifier * Time.deltaTime); // Move player based on the horizontal input
                    rigidbody.MovePosition(rigidbody.position + (transform.forward * v) * speed * modifier * Time.deltaTime); // Move player based on the vertical input
                    break;
                case ControlMode.FREE:
                    rigidbody.MovePosition(rigidbody.position + 4 * (Camera.main.transform.right * h) * speed * modifier * Time.deltaTime); // Move player based on the horizontal input
                    rigidbody.MovePosition(rigidbody.position + 4 * (Camera.main.transform.forward * v) * speed * modifier * Time.deltaTime); // Move player based on the vertical input
                    rigidbody.MovePosition(rigidbody.position + 4 * (transform.up * ud) * speed * modifier * Time.deltaTime); // Move player based on the vertical input
                    break;
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.1f); // Do a ray cast to see if the players collider is 0.1 away from the surface of something
    }
}