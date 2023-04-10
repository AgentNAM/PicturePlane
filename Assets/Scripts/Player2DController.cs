using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2DController : MonoBehaviour
{
    public GameObject player3D;
    public GameObject camera3D;
    public GameObject player2DSeenMarker;

    public GameObject environmentGray;
    public GameObject environmentGreen;
    public GameObject environmentMagenta;

    public float horizontalInput;

    public float speed = 0.3f;
    public float gravity = 0.03f;
    public float jumpStrength = 0.75f;
    public float jumpTimer = 0.15f;
    public float maxFallSpeed = 2.0f;
    public float maxWalkSlope = 0.15f;
    public float maxSlideSlope = 0.3f;

    public float velocityY;

    public float seenScale = 0.015f;
    public float seenCamDistance = 1.0f;

    public Vector3 seenPos;

    public float lcForwardInterval = 0.1f;
    public float lcBackwardInterval = 0.01f;

    public string closestSurfaceType;

    private bool modeIs3D = true;

    public bool onFloor = false;

    // Start is called before the first frame update
    void Start()
    {
        player2DSeenMarker.transform.localScale = new Vector3 (seenScale, seenScale, seenScale);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player presses E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Switch between 2D and 3D modes
            modeIs3D = !modeIs3D;
            // Reset Player2D's Y-velocity
            velocityY = 0.0f;

            // TODO: Have modeIs3D be shared across these scripts: Player2DController.cs, Player3DController.cs, FirstPersonCamera.cs
            // Then reintroduce the following code

            /*
            // If the current mode is 2D, or if the camera has an unobstructed view of Player2D
            if (!modeIs3D || !Physics.Linecast(Camera.main.transform.position, transform.position))
            {
                // Switch between 2D and 3D modes
                modeIs3D = !modeIs3D;
                // Reset Player2D's Y-velocity
                velocityY = 0.0f;
            }
            */
        }

        if (!modeIs3D)
        {
            // Push Player2D back to the surface closest to Player3D
            transform.position = FindClosestSurface(true);

            // --- HORIZONTAL MOVEMENT --- //
            Vector3 lastPos = transform.position; // Get Player2D's position before moving horizontally

            // Get horizontal input and move Player2D accordingly 
            horizontalInput = Input.GetAxisRaw("Horizontal");
            transform.Translate(Vector3.right * Time.deltaTime * SeenToReal(speed) * horizontalInput);

            // Check if Player2D's new position is inside a green surface
            if (FindClosestSurfaceType(transform.position) == "SurfaceGreen")
            {
                if (!MoveAlongSlope(maxWalkSlope, Vector3.up)) {
                    transform.position = lastPos;
                }
            }

            // --- VERTICAL MOVEMENT --- //
            lastPos = transform.position; // Get Player2D's position before moving vertically

            // Apply semi-realistic gravity
            if (velocityY > -maxFallSpeed)
            {
                velocityY -= gravity;
            }
            transform.Translate(Vector3.up * Time.deltaTime * SeenToReal(velocityY));

            // Check if Player2D's new position is inside a green surface
            if (FindClosestSurfaceType(transform.position) == "SurfaceGreen")
            {
                if (!MoveAlongSlope(maxSlideSlope, Vector3.right))
                {
                    velocityY = 0.0f;

                    if (RealToSeen(lastPos).y > RealToSeen(transform.position).y)
                    {
                        onFloor = true;
                        transform.position = lastPos;

                        if (IsInvoking("CoyoteTime"))
                        {
                            CancelInvoke("CoyoteTime");
                        }
                    }
                }
                
            }
            else
            {
                Invoke("CoyoteTime", jumpTimer);
            }

            if (Input.GetKeyDown(KeyCode.W) && onFloor)
            {
                velocityY = jumpStrength;
                onFloor = false;
                if (IsInvoking("CoyoteTime"))
                {
                    CancelInvoke("CoyoteTime");
                }
            }
        }

        // Set Player2D rotation and player2DSeenMarker rotation to face the camera
        transform.rotation = Camera.main.transform.rotation;
        player2DSeenMarker.transform.rotation = Camera.main.transform.rotation;

        // Set position of player2DSeenMarker
        seenPos = RealToSeen(transform.position);
        player2DSeenMarker.transform.position = seenPos + Camera.main.transform.position;

        // Set Player2D scale relative to its distance from the camera
        float realScale = SeenToReal(seenScale);
        transform.localScale = new Vector3(realScale, realScale, realScale);
    }

    float SeenToReal(float seenValue)
    {
        float realCamDistance = Vector3.Magnitude(Camera.main.transform.position - transform.position);
        return (seenValue / seenCamDistance) * realCamDistance;
    }

    Vector3 RealToSeen(Vector3 realValue)
    {
        return Vector3.ClampMagnitude(realValue - Camera.main.transform.position, seenCamDistance);
    }

    // Determine which point in 3D space the 2D player needs to be fixed
    Vector3 FindClosestSurface(bool pullBack)
    {
        LineRenderer projectionLine = gameObject.GetComponent<LineRenderer>();
        projectionLine.widthMultiplier = 0.01f;
        projectionLine.startWidth = 0;
        projectionLine.endWidth = transform.localScale.x;

        projectionLine.positionCount = 2;
        projectionLine.SetPositions(new Vector3[] { Camera.main.transform.position, player2DSeenMarker.transform.position });

        while (!Physics.Linecast(projectionLine.GetPosition(0), projectionLine.GetPosition(1)))
        {
            projectionLine.SetPosition(1,
                projectionLine.GetPosition(1) +
                Vector3.ClampMagnitude(player2DSeenMarker.transform.position - Camera.main.transform.position, lcForwardInterval));

            projectionLine.endWidth = transform.localScale.x;
        }

        closestSurfaceType = FindClosestSurfaceType(projectionLine.GetPosition(1));

        if (pullBack)
        {
            while (Physics.Linecast(projectionLine.GetPosition(0), projectionLine.GetPosition(1)))
            {
                projectionLine.SetPosition(1,
                    projectionLine.GetPosition(1) -
                    Vector3.ClampMagnitude(player2DSeenMarker.transform.position - Camera.main.transform.position, lcBackwardInterval));

                projectionLine.endWidth = transform.localScale.x;
            }
        }

        return projectionLine.GetPosition(1);
    }

    private string FindClosestSurfaceType(Vector3 endPoint)
    {
        RaycastHit hit;
        Physics.Linecast(Camera.main.transform.position, endPoint, out hit);
        if (hit.collider)
        {
            // Debug.Log(hit.collider.tag);
            return hit.collider.tag;
        }
        else
        {
            return null;
        }
    }

    bool MoveAlongSlope(float tolerance, Vector3 direction)
    {
        // Move Player2D slightly in positive direction
        transform.Translate(direction * Time.deltaTime * SeenToReal(tolerance));
        // Check if Player2D is still inside a green surface
        if (FindClosestSurfaceType(transform.position) == "SurfaceGreen")
        {
            // Move Player2D slightly in negative direction
            transform.Translate(-direction * Time.deltaTime * SeenToReal(tolerance * 2));
            // Check if Player2D is still inside a green surface
            if (FindClosestSurfaceType(transform.position) == "SurfaceGreen")
            {
                // The player cannot move smoothly along this slope
                return false;
            }
        }
        return true;
    }

    void CoyoteTime()
    {
        onFloor = false;
    }
}
