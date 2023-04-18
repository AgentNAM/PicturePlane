using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player2DController : MonoBehaviour
{
    public GameObject player2DSeenMarker;
    public GameObject cameraScreenArea;

    public Material unobstructedView;
    public Material obstructedView;

    public bool modeIs3D = true;

    private float horizontalInput;

    private float speed = 0.3f;
    private float gravity = 0.03f;
    private float jumpStrength = 0.75f;
    private float jumpTimer = 0.15f;
    private float maxFallSpeed = 2.0f;
    private float maxWalkSlope = 0.15f;
    private float maxSlideSlope = 0.075f;

    private float velocityY;

    private float seenScale = 0.015f;
    private float seenCamDistance = 1.0f;

    private Vector3 lastSafePos;

    private float lcForwardInterval = 0.1f;
    private float lcBackwardInterval = 0.01f;

    private string closestSurfaceType;

    private bool onFloor = false;

    private string[] surfaces = new string[] { "SurfaceGray", "SurfaceGreen", "SurfaceMagenta" };

    // Start is called before the first frame update
    void Start()
    {
        player2DSeenMarker.transform.localScale = new Vector3 (seenScale, seenScale, seenScale);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(FindClosestSurfaceType(transform.position));
        }
        // --- PERSPECTIVE SWITCHING --- //
        // Check if the player presses E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (modeIs3D)
            {
                // Make sure Player2D is on screen
                if (IsPlayerOnScreen())
                {
                    // Make sure the camera has an unobstructed view of Player2D
                    if (FindClosestSurfaceType(transform.position) == null)
                    {
                        SwitchPerspectives(true);
                    }
                }
            }
            else
            {
                SwitchPerspectives(false);
            }
        }

        // Set Player2D rotation and player2DSeenMarker rotation to face the camera
        transform.rotation = Camera.main.transform.rotation;
        player2DSeenMarker.transform.rotation = Camera.main.transform.rotation;

        // Set position of player2DSeenMarker
        player2DSeenMarker.transform.position = RealToSeen(transform.position) + Camera.main.transform.position;

        // Set Player2D scale relative to its distance from the camera
        float realScale = SeenToReal(seenScale, transform.position);
        transform.localScale = new Vector3(realScale, realScale, realScale);

        // Indicate whether the camera's view of Player2D is obstructed
        if (modeIs3D)
        {
            if (FindClosestSurfaceType(transform.position) == null)
            {
                player2DSeenMarker.GetComponent<Renderer>().material = unobstructedView;
            }
            else
            {
                player2DSeenMarker.GetComponent<Renderer>().material = obstructedView;
            }
        }

        // --- 2D MOVEMENT --- //
        if (!modeIs3D)
        {
            // Push Player2D back to the surface closest to Player3D
            transform.position = FindClosestSurface(player2DSeenMarker.transform.position);

            // --- HORIZONTAL MOVEMENT --- //
            Vector3 lastPos = transform.position; // Get Player2D's position before moving horizontally

            // Get horizontal input and move Player2D accordingly 
            horizontalInput = Input.GetAxisRaw("Horizontal");
            transform.Translate(Vector3.right * Time.deltaTime * SeenToReal(speed, transform.position) * horizontalInput);

            ReSyncPlayerPositions();

            // Check if Player2D's new position is inside a green surface
            if (closestSurfaceType == "SurfaceGreen")
            {
                if (!MoveToFreeSpace(maxWalkSlope, Vector3.up)) {
                    transform.position = lastPos;
                }
            }

            ReSyncPlayerPositions();

            // --- VERTICAL MOVEMENT --- //
            // TODO: Prevent velocityY from being affected by actual distance
            lastPos = transform.position; // Get Player2D's position before moving vertically

            // Apply semi-realistic gravity
            if (velocityY > -maxFallSpeed)
            {
                velocityY -= gravity;
            }

            if (Input.GetKeyDown(KeyCode.W) && onFloor)
            {
                velocityY = jumpStrength;
                onFloor = false;
                if (IsInvoking("CoyoteTime"))
                {
                    CancelInvoke("CoyoteTime");
                }
                Debug.Log($"{velocityY}, {SeenToReal(velocityY, transform.position)}");
            }

            transform.Translate(Vector3.up * Time.deltaTime * SeenToReal(velocityY, transform.position));

            ReSyncPlayerPositions();

            // Check if Player2D's new position is inside a green surface
            if (closestSurfaceType == "SurfaceGreen")
            {
                if (!MoveToFreeSpace(maxSlideSlope, Vector3.right))
                {
                    if (RealToSeen(lastPos).y > RealToSeen(transform.position).y)
                    {
                        onFloor = true;

                        if (IsInvoking("CoyoteTime"))
                        {
                            CancelInvoke("CoyoteTime");
                        }
                    }

                    velocityY = 0.0f;
                    transform.position = lastPos;
                }
                
            }
            else
            {
                Invoke("CoyoteTime", jumpTimer);
            }

            ReSyncPlayerPositions();

            // --- MAGENTA SURFACE COLLISION --- //
            if (closestSurfaceType == "SurfaceMagenta" || !IsPlayerOnScreen())
            {
                transform.position = lastSafePos;
                SwitchPerspectives(false);
            }
        }
    }

    bool IsPlayerOnScreen()
    {
        cameraScreenArea.SetActive(true);
        bool onScreen = FindClosestSurfaceType(transform.position) == "CameraScreenArea";
        cameraScreenArea.SetActive(false);
        return onScreen;
    }

    void SwitchPerspectives(bool switchingTo2d)
    {
        if (switchingTo2d)
        {
            lastSafePos = transform.position;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
        // Switch between 2D and 3D modes
        modeIs3D = !modeIs3D;
        // Reset Player2D's Y-velocity
        velocityY = 0.0f;
    }

    void ReSyncPlayerPositions()
    {
        player2DSeenMarker.transform.position = RealToSeen(transform.position) + Camera.main.transform.position;
        transform.position = FindClosestSurface(player2DSeenMarker.transform.position);
    }

    float SeenToReal(float seenValue, Vector3 referencePoint)
    {
        float realCamDistance = Vector3.Magnitude(Camera.main.transform.position - referencePoint);
        return (seenValue / seenCamDistance) * realCamDistance;
    }

    Vector3 RealToSeen(Vector3 realValue)
    {
        return Vector3.ClampMagnitude(realValue - Camera.main.transform.position, seenCamDistance);
    }

    // Determine which point in 3D space the 2D player needs to be fixed
    Vector3 FindClosestSurface(Vector3 aimPos)
    {
        Vector3 castPos = aimPos;

        Vector3[] localDirs = new Vector3[]
        {
            transform.right,
            -transform.right,
            transform.up,
            -transform.up
        };

        string[] collidingSurfaceTypes = new string[localDirs.Length];

        // Extend a Linecast until it hits something
        while (collidingSurfaceTypes.Contains(null))
        {
            castPos += Vector3.ClampMagnitude(aimPos - Camera.main.transform.position, lcForwardInterval);

            collidingSurfaceTypes = FindCollidingSurfaceTypes(castPos, localDirs, collidingSurfaceTypes);
        }

        // Pull the Linecast back until it stops hitting things
        while (surfaces.Contains(FindClosestSurfaceType(castPos)))
        {
            collidingSurfaceTypes = FindCollidingSurfaceTypes(castPos, localDirs, collidingSurfaceTypes);

            castPos -= Vector3.ClampMagnitude(aimPos - Camera.main.transform.position, lcBackwardInterval);
        }

        closestSurfaceType = HighestPrioritySurface(collidingSurfaceTypes);
        return castPos;
    }

    string[] FindCollidingSurfaceTypes(Vector3 basePos, Vector3[] localDirs, string[] oldSurfaceTypes)
    {
        string[] collidingSurfaceTypes = new string[localDirs.Length];
        Array.Copy(oldSurfaceTypes, collidingSurfaceTypes, collidingSurfaceTypes.Length);

        for (int i = 0; i < localDirs.Length; i++)
        {
            Vector3 localDir = localDirs[i];
            Vector3 offset = localDir * SeenToReal(seenScale / 2, basePos);
            Vector3 offsetPos = basePos + offset;

            string offsetSurfaceType = FindClosestSurfaceType(offsetPos);

            if (surfaces.Contains(offsetSurfaceType))
            {
                collidingSurfaceTypes[i] = offsetSurfaceType;
            }
        }

        return collidingSurfaceTypes;
    }

    string HighestPrioritySurface(string[] surfaceTypes)
    {
        string highestSurface = null;
        foreach (string surfaceType in surfaceTypes)
        {
            if (highestSurface == null || Array.IndexOf(surfaces, surfaceType) > Array.IndexOf(surfaces, highestSurface))
            {
                highestSurface = surfaceType;
            }
        }

        return highestSurface;
    }

    private string FindClosestSurfaceType(Vector3 endPoint)
    {
        // Do a Linecast between the camera and Player2D, then return the first GameObject hit by the Linecast
        RaycastHit hit;
        Physics.Linecast(Camera.main.transform.position, endPoint, out hit);

        // Return the tag of the first GameObject hit by the Linecast, or return null if the Linecast did not hit anything
        if (hit.collider)
        {
            return hit.collider.tag;
        }
        else
        {
            return null;
        }
    }

    bool MoveToFreeSpace(float tolerance, Vector3 direction)
    {
        // Move Player2D slightly in positive direction
        transform.Translate(direction * Time.deltaTime * SeenToReal(tolerance, transform.position));
        ReSyncPlayerPositions();
        // Check if Player2D is still inside a green surface
        if (closestSurfaceType == "SurfaceGreen")
        {
            // Move Player2D slightly in negative direction
            transform.Translate(-direction * Time.deltaTime * SeenToReal(tolerance * 2, transform.position));
            ReSyncPlayerPositions();
            // Check if Player2D is still inside a green surface
            if (closestSurfaceType == "SurfaceGreen")
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
