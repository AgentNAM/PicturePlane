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



            // inputDir.Normalize();

            // Move 3D player based on WASD input
            // TODO: Try to implement player3DRb.AddForce()
            // TODO: Make gravity & smooth movement work
            /*
            player3DRb.AddForce(transform.right * inputX * speed, ForceMode.VelocityChange);
            player3DRb.AddForce(transform.forward * inputZ * speed, ForceMode.VelocityChange);
            */

            // Vector3 transformDir = (transform.right + transform.forward).normalized;
            // Vector3 forceDir = new Vector3(transformDir.x * inputX, 0, transformDir.z * inputZ);
            /*
            if (new Vector2(player3DRb.velocity.x, player3DRb.velocity.z).magnitude < maxSpeed)
            {
                player3DRb.AddRelativeForce(inputDir * moveForce);
            }

            if (inputDir.magnitude == 0)
            {
                player3DRb.velocity *= 0.75f;
            }
            */
            // player3DRb.AddForce(forceDir * speed, ForceMode.VelocityChange);

            /*
            Vector3 forceX = transform.right * inputX;
            Vector3 forceZ = transform.forward * inputZ;

            Vector3 totalForce = (forceX + forceZ).normalized * speed;
            player3DRb.velocity = new Vector3(totalForce.x, player3DRb.velocity.y, totalForce.z);
            */
            // player3DRb.velocity = player3DRb.velocity.normalized * speed;

            /*
            transform.Translate(inputZ * speed * Time.deltaTime * Vector3.forward);
            transform.Translate(inputX * speed * Time.deltaTime * Vector3.right);
            */
        }
        else
        {
            // If the current mode is 2D, set Player3D's velocity to zero
            player3DRb.velocity = Vector3.zero;
        }
    }
}
