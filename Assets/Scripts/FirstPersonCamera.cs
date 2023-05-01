using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    // First-person camera code: https://www.youtube.com/watch?v=f473C43s8nE

    public GameObject player3D;
    public GameObject player2D;

    public float sensitivityX;
    public float sensitivityY;

    private float xRotation;
    private float yRotation;

    private Player2DController player2DControllerScript;

    // Start is called before the first frame update
    void Start()
    {
        player2DControllerScript = GameObject.Find("Player2DRealMarker").GetComponent<Player2DController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.rotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (player2DControllerScript.modeIs3D)
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
