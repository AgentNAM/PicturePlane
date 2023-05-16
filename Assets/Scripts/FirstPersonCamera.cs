using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    // First-person camera starter code: https://www.youtube.com/watch?v=f473C43s8nE

    public GameObject player3D;
    public GameObject player2D;

    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;
    public float sensitivity = 100.0f;

    private PlayerStateManager playerStateManager;

    private float xRotation;
    private float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        playerStateManager = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();

        // Prevent camera rotation from being reset
        yRotation = transform.eulerAngles.y;
        xRotation = (transform.eulerAngles.x + 180f) % 360f - 180f;

        // Set sensitivity slider value in options menu
        sensitivitySlider.value = sensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is not paused
        if (!playerStateManager.paused)
        {
            // If the player is in 3D mode
            if (playerStateManager.modeIs3D)
            {
                // Get mouse input
                float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
                float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

                // Player can look left and right
                yRotation += mouseX;

                // Player can look up and down
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                // Set camera and player rotations
                transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
                player3D.transform.rotation = Quaternion.Euler(0, yRotation, 0);

                // Set camera position at player position
                transform.position = player3D.transform.position;
            }
        }
    }

    // Sensitivity slider logic
    public void OnSensitivityChanged()
    {
        sensitivity = sensitivitySlider.value;
        sensitivityText.text = "Sensitivity: " + sensitivity;
    }
}
