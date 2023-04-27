using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2DController : MonoBehaviour
{
    public GameObject seenMarker;
    public GameObject cameraScreenArea;

    public Material unobstructedView;
    public Material obstructedView;

    public bool modeIs3D = false;

    private float horizontalInput;

    private float speed = 0.3f;
    private float gravity = 2.5f;
    private float jumpStrength = 0.75f;
    private float coyoteTime = 0.15f;
    private float maxFallSpeed = 2.0f;
    private float maxWalkSlope = 0.15f;
    private float maxSlideSlope = 0.075f;

    private float velocityY;

    private float seenScale = 0.015f;
    private float seenCamDistance = 1.0f;

    private Vector3 lastSafePos;

    private Vector3 anchorPos;
    private Vector3 anchorDir;

    private float lcForwardInterval = 1.0f;
    private float lcBackwardInterval = 0.1f;

    public bool onFloor = false;

    private readonly string[] surfaces = new string[] { "SurfaceGray", "EntryZone", "SurfaceGreen", "SurfaceMagenta", "Goal" };

    // Start is called before the first frame update
    void Start()
    {
        seenMarker.transform.localScale = new Vector3 (seenScale, seenScale, seenScale);
        SwitchPerspectives();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // --- PERSPECTIVE SWITCHING --- //
        // Check if the player presses E
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchPerspectives();
        }

        // Set Player2D rotation and seenMarker rotation to face the camera
        transform.rotation = Camera.main.transform.rotation;
        seenMarker.transform.rotation = Camera.main.transform.rotation;

        // Set position of seenMarker
        seenMarker.transform.position = RealToSeen(transform.position) + Camera.main.transform.position;

        // Set Player2D scale relative to its distance from the camera
        float realScale = SeenToReal(seenScale, transform.position);
        transform.localScale = new Vector3(realScale, realScale, realScale);

        // Indicate whether the camera's view of Player2D is obstructed
        if (modeIs3D)
        {
            transform.position = GetOffsetPos(anchorPos, transform.up, seenScale / 2);
            transform.position = GetOffsetPos(transform.position, anchorDir, seenScale / 2);
            if (IsViewClear())
            {
                seenMarker.GetComponent<Renderer>().material = unobstructedView;
            }
            else
            {
                seenMarker.GetComponent<Renderer>().material = obstructedView;
            }
        }

        // --- 2D MOVEMENT --- //
        if (!modeIs3D)
        {
            // Push Player2D back to the surface closest to Player3D
            ReSyncRealPosition();

            // --- HORIZONTAL MOVEMENT --- //
            Vector3 lastSeenPos = seenMarker.transform.position; // Get Player2D's position before moving horizontally

            horizontalInput = Input.GetAxisRaw("Horizontal"); // Get horizontal input

            seenMarker.transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput); // Apply horizontal movement

            ReSyncRealPosition();

            // Check if Player2D's new position is inside a green surface
            if (GetScaledCollision(seenMarker.transform.position) == "SurfaceGreen")
            {
                // If Player2D is colliding with a sloped floor/ceiling, allow them to move along the slope
                MoveAlongSlope(Vector3.up, maxWalkSlope, out bool isSlope);

                // If Player2D is colliding with a wall, reset their position
                if (!isSlope) {
                    seenMarker.transform.position = lastSeenPos;
                }
            }

            ReSyncRealPosition();

            // --- VERTICAL MOVEMENT --- //
            lastSeenPos = seenMarker.transform.position; // Get Player2D's position before moving vertically

            // Player2D accelerates downwards
            if (velocityY > -maxFallSpeed)
            {
                velocityY -= gravity * Time.deltaTime;
            }

            // If Player2D is on the floor and the player presses W, jump
            if (Input.GetKeyDown(KeyCode.W) && onFloor)
            {
                velocityY = jumpStrength;
                onFloor = false;

                // End coyote time immediately
                if (IsInvoking("DisableJump"))
                {
                    CancelInvoke("DisableJump");
                }
            }

            // Apply vertical movement
            seenMarker.transform.Translate(Vector3.up * Time.deltaTime * velocityY);

            ReSyncRealPosition();

            // Check if Player2D's new position is inside a green surface
            if (GetScaledCollision(seenMarker.transform.position) == "SurfaceGreen")
            {
                // If Player2D is colliding with a sloped wall, allow them to move along the slope
                MoveAlongSlope(Vector3.right, maxSlideSlope, out bool isSlope);

                // If Player2D is colliding with a floor/ceiling...
                if (!isSlope)
                {
                    // Check if Player2D is colliding with a floor
                    if (GetScaledCollision(GetOffsetPos(seenMarker.transform.position, -transform.up, seenScale / 2)) == "SurfaceGreen")
                    {
                        onFloor = true;

                        // End coyote time immediately
                        if (IsInvoking("DisableJump"))
                        {
                            CancelInvoke("DisableJump");
                        }
                    }

                    // Reset Player2D's y-velocity and position
                    velocityY = 0.0f;
                    seenMarker.transform.position = lastSeenPos;
                }
            }
            else
            {
                // When Player2D leaves the ground, wait briefly before removing their ability to jump
                Invoke("DisableJump", coyoteTime);
            }

            ReSyncRealPosition();

            // --- MAGENTA SURFACE COLLISION --- //
            // If Player2D touches a magenta surface or exits the screen, send Player2D back to their last safe position
            if (GetScaledCollision(seenMarker.transform.position) == "SurfaceMagenta" || !IsPlayerOnScreen())
            {
                ReSyncSeenPosition();
                SwitchPerspectives();
            }

            // --- GOAL LOGIC --- //
            // If Player2D touches the goal, return to hub world
            if (GetScaledCollision(seenMarker.transform.position) == "Goal")
            {
                SwitchPerspectives();
                SceneManager.LoadScene("Hub", LoadSceneMode.Single);
            }
        }
    }

    // Switch between 2D and 3D perspectives
    void SwitchPerspectives()
    {
        if (modeIs3D) // If the player is switching from 3D to 2D
        {
            // Make sure Player2D is on screen
            if (IsPlayerOnScreen())
            {
                // Make sure the camera has an unobstructed view of Player2D
                if (IsViewClear())
                {
                    // Save the player's current position in case they die
                    lastSafePos = transform.position;

                    // Switch to 2D
                    modeIs3D = false;
                }
            }
        }
        else // If the player is switching from 2D to 3D
        {
            // If the player is in front of a level entry zone, enter the level
            if (FindSurfaceTypeAtPoint(transform.position) == "EntryZone")
            {
                EnterLevel();
            }

            // Push the player back to just in front of the wall they are on
            transform.position = FindClosestPoint(seenMarker.transform.position);
            anchorPos = GetOffsetPos(transform.position, -transform.up, seenScale / 2);
            Vector3 anchorPosFront = GetOffsetPos(anchorPos, -transform.forward, seenScale / 2);
            anchorDir = (anchorPosFront - anchorPos).normalized;

            // Prevent the player from saving their jump status between perspective shifts
            onFloor = false;

            // Switch to 3D
            modeIs3D = true;
        }

        // Reset Player2D's Y-velocity
        velocityY = 0.0f;
    }

    // Check if Player2D is on screen
    bool IsPlayerOnScreen()
    {
        cameraScreenArea.SetActive(true);
        bool onScreen = FindSurfaceTypeAtPoint(transform.position) == "CameraScreenArea";
        cameraScreenArea.SetActive(false);
        return onScreen;
    }

    // Check if player's view of Player2D is unobstructed
    bool IsViewClear()
    {
        Vector3[] localDirs = new Vector3[]
        {
            Vector3.zero,
            transform.right,
            transform.right + transform.up,
            transform.up,
            -transform.right + transform.up,
            -transform.right,
            -transform.right - transform.up,
            -transform.up,
            transform.right - transform.up
        };

        // For each local direction
        foreach (Vector3 localDir in localDirs)
        {
            // Get the position of the player offset in the local direction (player edges and corners)
            Vector3 offsetPos = GetOffsetPos(transform.position, localDir, seenScale / 2);
            
            Vector3 backDir = (offsetPos - Camera.main.transform.position).normalized;
            Vector3 pointToCheck = offsetPos - (backDir * transform.localScale.y);

            if (FindSurfaceTypeAtPoint(pointToCheck) != null)
            {
                return false;
            }
        }

        return true;
    }

    // Enter a level
    void EnterLevel()
    {
        // Do a Linecast between the camera and Player2D, then return the first GameObject hit by the Linecast
        Physics.Linecast(Camera.main.transform.position, transform.position, out RaycastHit hit);

        // If Player2D is standing in front of an entry zone
        if (hit.collider.CompareTag("EntryZone"))
        {
            // Get the name of the level to enter
            string levelName = hit.collider.gameObject.GetComponent<EnterLevel>().levelToLoad.name;

            // Make sure the level exists before loading the level
            if (SceneManager.GetSceneByName(levelName) == null)
            {
                Debug.Log($"Level \"{levelName}\" does not exist.");
            }
            else
            {
                SceneManager.LoadScene(levelName, LoadSceneMode.Single);
            }
        }
    }

    void ReSyncRealPosition()
    {
        transform.position = FindClosestPoint(seenMarker.transform.position);
    }

    void ReSyncSeenPosition()
    {
        seenMarker.transform.position = RealToSeen(lastSafePos) + Camera.main.transform.position;
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

    // /*
    // vvvvvvvvvvvvvvvvvvvvvvvvvv

    private string FindSurfaceTypeAtPoint(Vector3 endPoint)
    {
        // Do a Linecast between the camera and Player2D, then return the first GameObject hit by the Linecast
        Physics.Linecast(Camera.main.transform.position, endPoint, out RaycastHit hit);

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

    Vector3 FindClosestPoint(Vector3 aimPos, float pullBack = 0.0f)
    {
        Vector3 castPos = aimPos;
        Vector3 aimDir = (aimPos - Camera.main.transform.position).normalized;

        // Extend the cast until a surface is hit
        while (!surfaces.Contains(FindSurfaceTypeAtPoint(castPos)))
        {
            castPos += Vector3.ClampMagnitude(aimDir, lcForwardInterval);
        }

        // Pull back until no surface is hit
        while (surfaces.Contains(FindSurfaceTypeAtPoint(castPos)))
        {
            castPos -= Vector3.ClampMagnitude(aimDir, lcBackwardInterval);
        }

        castPos += Vector3.ClampMagnitude(aimDir, lcBackwardInterval);

        if (pullBack > 0.0f)
        {
            castPos -= Vector3.ClampMagnitude(aimDir, pullBack);
        }

        return castPos;
    }

    Vector3 GetOffsetPos(Vector3 basePos, Vector3 localDir, float offsetAmount)
    {
        Vector3 offset = localDir * SeenToReal(offsetAmount, basePos);
        return basePos + offset;
    }

    string FindOffsetSurfaceType(Vector3 basePos, Vector3 localDir, float offsetAmount)
    {
        Vector3 offsetPos = GetOffsetPos(basePos, localDir, offsetAmount);
        return FindSurfaceTypeAtPoint(FindClosestPoint(offsetPos));
    }

    string GetScaledCollision(Vector3 basePos)
    {
        /*
        Vector3[] localDirs = new Vector3[]
        {
            Vector3.zero,
            transform.right,
            -transform.right,
            transform.up,
            -transform.up
        };
        */

        Vector3[] localDirs = new Vector3[]
        {
            Vector3.zero,
            transform.right,
            transform.right + transform.up,
            transform.up,
            -transform.right + transform.up,
            -transform.right,
            -transform.right - transform.up,
            -transform.up,
            transform.right - transform.up
        };

        string[] collidingSurfaceTypes = new string[localDirs.Length];

        for (int i = 0; i < localDirs.Length; i++)
        {
            Vector3 localDir = localDirs[i];
            collidingSurfaceTypes[i] = FindOffsetSurfaceType(basePos, localDir, seenScale / 2);
        }

        string prioritizedSurfaceType = GetPrioritizedSurfaceType(collidingSurfaceTypes);

        return prioritizedSurfaceType;
    }

    string GetPrioritizedSurfaceType(string[] surfaceTypes)
    {
        string highestSurface = null;

        foreach (string surfaceType in surfaceTypes)
        {
            if (Array.IndexOf(surfaces, surfaceType) > Array.IndexOf(surfaces, highestSurface))
            {
                highestSurface = surfaceType;
            }
        }

        return highestSurface;
    }

    void MoveAlongSlope(Vector3 worldDir, float tolerance, out bool canMove)
    {
        canMove = true;
        // Move Player2D slightly in positive direction
        // transform.Translate(direction * Time.deltaTime * SeenToReal(tolerance, transform.position));
        seenMarker.transform.Translate(worldDir * Time.deltaTime * tolerance);
        ReSyncRealPosition();
        // Check if Player2D is still inside a green surface
        if (GetScaledCollision(seenMarker.transform.position) == "SurfaceGreen")
        {
            // Move Player2D slightly in negative direction
            // transform.Translate(-direction * Time.deltaTime * SeenToReal(tolerance * 2, transform.position));
            seenMarker.transform.Translate(-worldDir * Time.deltaTime * tolerance * 2);
            ReSyncRealPosition();
            // Check if Player2D is still inside a green surface
            if (GetScaledCollision(seenMarker.transform.position) == "SurfaceGreen")
            {
                // The player cannot move smoothly along this slope
                canMove = false;
            }
        }

        /*
        Vector3 offsetPosP = GetOffsetPos(seenMarker.transform.position, slopeDir, tolerance);
        Vector3 offsetPosN = GetOffsetPos(seenMarker.transform.position, -slopeDir, tolerance);

        if (GetScaledCollision(offsetPosP) != "SurfaceGreen")
        {
            if (worldDir == Vector3.up)
            {
                Debug.Log($"Free space + {GetScaledCollision(offsetPosP)}, {worldDir}");
            }

            seenMarker.transform.Translate(worldDir * tolerance * Time.deltaTime);
            ReSyncRealPosition();
            canMove = true;
        }
        else if (GetScaledCollision(offsetPosN) != "SurfaceGreen")
        {
            if (worldDir == Vector3.up)
            {
                Debug.Log($"Free space - {GetScaledCollision(offsetPosN)}, {-worldDir}");
            }

            seenMarker.transform.Translate(-worldDir * tolerance * Time.deltaTime);
            ReSyncRealPosition();
            canMove = true;
        }
        else
        {
            if (worldDir == Vector3.up)
            {
                Debug.Log("Free space 0");
            }
            canMove = false;
        }
        */
    }

    // ^^^^^^^^^^^^^^^^^^^
    // */

    /*
    // Determine which point in 3D space the 2D player needs to be fixed
    Vector3 FindClosestSurface(Vector3 aimPos)
    {
        Vector3 castPos = aimPos;

        Vector3[] localDirs = new Vector3[]
        {
            Vector3.zero,
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

            collidingSurfaceTypes = FindCollidingSurfaceTypes(castPos, localDirs, collidingSurfaceTypes, true);
        }

        // Pull the Linecast back until it stops hitting things
        while (surfaces.Contains(FindClosestSurfaceType(castPos)))
        {
            collidingSurfaceTypes = FindCollidingSurfaceTypes(castPos, localDirs, collidingSurfaceTypes, false);

            castPos -= Vector3.ClampMagnitude(aimPos - Camera.main.transform.position, lcBackwardInterval);
        }

        closestSurfaceType = HighestPrioritySurface(collidingSurfaceTypes);
        return castPos;
    }

    string[] FindCollidingSurfaceTypes(Vector3 basePos, Vector3[] localDirs, string[] oldSurfaceTypes, bool skipNonNull)
    {
        string[] newSurfaceTypes = new string[localDirs.Length];
        Array.Copy(oldSurfaceTypes, newSurfaceTypes, newSurfaceTypes.Length);

        for (int i = 0; i < localDirs.Length; i++)
        {
            if (skipNonNull && newSurfaceTypes[i] != null)
            {
                continue;
            }

            Vector3 localDir = localDirs[i];
            Vector3 offset = localDir * SeenToReal(seenScale / 2, basePos);
            Vector3 offsetPos = basePos + offset;

            string offsetSurfaceType = FindClosestSurfaceType(offsetPos);

            if (surfaces.Contains(offsetSurfaceType))
            {
                newSurfaceTypes[i] = offsetSurfaceType;

                if (localDir == -transform.up && offsetSurfaceType == "SurfaceGreen")
                {
                    onFloor = true;

                    if (IsInvoking("DisableJump"))
                    {
                        CancelInvoke("DisableJump");
                    }
                }
            }
        }

        return newSurfaceTypes;
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
        // transform.Translate(direction * Time.deltaTime * SeenToReal(tolerance, transform.position));
        seenMarker.transform.Translate(direction * Time.deltaTime * tolerance);
        ReSyncRealPosition();
        // Check if Player2D is still inside a green surface
        if (closestSurfaceType == "SurfaceGreen")
        {
            // Move Player2D slightly in negative direction
            // transform.Translate(-direction * Time.deltaTime * SeenToReal(tolerance * 2, transform.position));
            seenMarker.transform.Translate(-direction * Time.deltaTime * tolerance * 2);
            ReSyncRealPosition();
            // Check if Player2D is still inside a green surface
            if (closestSurfaceType == "SurfaceGreen")
            {
                // The player cannot move smoothly along this slope
                return false;
            }
        }
        return true;
    }
    */

    // Removes the player's ability to jump after a set amount of time
    void DisableJump()
    {
        onFloor = false;
    }
}
