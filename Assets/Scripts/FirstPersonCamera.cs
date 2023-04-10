using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    // First-person camera code: https://www.youtube.com/watch?v=f473C43s8nE

    public GameObject player3D;

    public float sensitivityX;
    public float sensitivityY;

    private float xRotation;
    private float yRotation;

    private bool modeIs3D = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Switch between 2D and 3D modes when the player presses E
        if (Input.GetKeyDown(KeyCode.E))
        {
            modeIs3D = !modeIs3D;
        }

        if (modeIs3D)
        {
            // Get mouse input
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            player3D.transform.rotation = Quaternion.Euler(0, yRotation, 0);

            transform.position = player3D.transform.position;
        }
    }
}
