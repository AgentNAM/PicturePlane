// Necolai McIntosh
// Picture Plane
// April 10, 2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3DController : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;

    public float speed;

    private bool modeIs3D = true;

    // Start is called before the first frame update
    void Start()
    {
        
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
            // Get WASD input
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            // Move 3D player based on WASD input
            transform.Translate(Vector3.forward * Time.deltaTime * speed * verticalInput);
            transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);
        }
        else
        {
            // If the current mode is 2D, set Player3D's velocity to zero
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
        }
    }
}
