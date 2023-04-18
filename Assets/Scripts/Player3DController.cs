// Necolai McIntosh
// Picture Plane
// April 10, 2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3DController : MonoBehaviour
{
    public GameObject player2D;

    public float horizontalInput;
    public float verticalInput;

    public float speed;

    private Rigidbody player3DRb;

    private Player2DController player2DControllerScript;

    // Start is called before the first frame update
    void Start()
    {
        player2DControllerScript = GameObject.Find("Player2DRealMarker").GetComponent<Player2DController>();
        player3DRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player2DControllerScript.modeIs3D)
        {
            // Get WASD input
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            // Move 3D player based on WASD input
            // TODO: Try to implement player3DRb.AddForce()
            transform.Translate(verticalInput * speed * Time.deltaTime * Vector3.forward);
            transform.Translate(horizontalInput * speed * Time.deltaTime * Vector3.right);
        }
        else
        {
            // If the current mode is 2D, set Player3D's velocity to zero
            player3DRb.velocity = Vector3.zero;
        }
    }
}
