using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float camRotationSpeed = 10.0f;

    bool right_mouse_clicked = false;
    Vector3 savedMousePosition;
    Quaternion savedRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ControlCamera()
    {
        MoveCamera();
        RotateCamera();
    }

    void MoveCamera()
    {
        // wasd movement
        float height = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        Vector3 move_dir = new(Input.GetAxis("Horizontal"), height, Input.GetAxis("Vertical"));

        transform.position += moveSpeed * Time.deltaTime * transform.TransformVector(move_dir);

    }

    void RotateCamera()
    {
        if (Input.GetMouseButtonDown(1))
        {
            right_mouse_clicked = true;
            savedMousePosition = Input.mousePosition;
            savedRotation = transform.rotation;
        }

        if (right_mouse_clicked && Input.GetMouseButton(1))
        {
            Vector3 mouse_delta = Input.mousePosition - savedMousePosition;

            float yaw = mouse_delta.x * camRotationSpeed;
            float pitch = -mouse_delta.y * camRotationSpeed;

            Quaternion yawRot = Quaternion.AngleAxis(yaw, Vector3.up);
            Quaternion pitchRot = Quaternion.AngleAxis(pitch, transform.right);

            Quaternion newRot = yawRot * pitchRot * savedRotation;

            // Convert to euler, sanitize roll
            Vector3 euler = newRot.eulerAngles;
            euler.z = 0f; // eliminate roll

            transform.rotation = Quaternion.Euler(euler);
        }

        if (Input.GetMouseButtonUp(1))
        {
            right_mouse_clicked = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
