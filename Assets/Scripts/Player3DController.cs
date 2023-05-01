// Necolai McIntosh
// Picture Plane
// April 10, 2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player3DController : MonoBehaviour
{
    public GameObject player2D;

    public float inputX;
    public float inputZ;

    public float speed = 10.0f;

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
            inputX = Input.GetAxis("Horizontal");
            inputZ = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(inputX, 0, inputZ);
            inputDir = Vector3.ClampMagnitude(inputDir, 1);

            Vector3 moveDir = (transform.right * inputDir.x) + (transform.forward * inputDir.z);
            moveDir *= speed;
            moveDir.y = player3DRb.velocity.y;

            player3DRb.velocity = moveDir;
        }
        else
        {
            // If the current mode is 2D, set Player3D's velocity to zero
            player3DRb.velocity = Vector3.zero;
        }
    }
}
