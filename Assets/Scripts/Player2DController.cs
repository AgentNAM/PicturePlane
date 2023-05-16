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

    public ParticleSystem deathParticle;

    private AudioSource playerAudioSource;
    public AudioClip jumpSound;
    public AudioClip deathSound;

    public bool onFloor = false;
    public bool isRespawning = false;

    private PlayerStateManager playerStateManager;

    private float horizontalInput;

    private float velocityY;

    private float seenScale = 0.02f;
    private float seenCamDistance = 1.0f;

    private float lcForwardInterval = 1.0f;
    private float lcBackwardInterval = 0.1f;

    private float speed = 0.35f;
    private float gravity = 2.5f;
    private float jumpStrength = 0.75f;
    private float coyoteTime = 0.15f;
    private float respawnTime = 1.0f;
    private float maxFallSpeed = 2.0f;
    private float maxWalkSlope = 0.15f;
    private float maxSlideSlope = 0.075f;

    private Vector3 lastSafePos;

    private Vector3 anchorPos;

    private readonly string[] surfaces = new string[] { "SurfaceGray", "EntryZone", "SurfaceGreen", "SurfaceMagenta", "Goal" };

    // Start is called before the first frame update
    void Start()
    {
        playerStateManager = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();

        seenMarker.transform.localScale = new Vector3 (seenScale, seenScale, seenScale);

        // Set Player2D rotation and seenMarker rotation to face the camera
        transform.rotation = Camera.main.transform.rotation;
        seenMarker.transform.rotation = Camera.main.transform.rotation;

        // Set position of seenMarker
        Vector3 seenStartOffset1 = Camera.main.transform.forward * seenCamDistance;
        Vector3 seenStartOffset2 = Camera.main.transform.up * seenScale / 2;
        seenMarker.transform.position = Camera.main.transform.position + seenStartOffset1 + seenStartOffset2;

        // Push the player back to just in front of the wall they are on
        transform.position = FindClosestPoint(seenMarker.transform.position, lcBackwardInterval);
        anchorPos = GetOffsetPos(transform.position, -transform.up, seenScale / 2);

        // Save the player's current position in case they die
        lastSafePos = anchorPos;

        playerStateManager.UpdateScreenBorderMaterial();

        // Set up audio source
        playerAudioSource = seenMarker.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!playerStateManager.paused)
        {
            // --- PERSPECTIVE SWITCHING --- //
            // Check if the player presses E
            if (Input.GetKeyDown(KeyCode.E) && !isRespawning)
            {
                SwitchPerspectives();
            }

            // Set Player2D rotation and seenMarker rotation to face the camera
            transform.rotation = Camera.main.transform.rotation;
            seenMarker.transform.rotation = Camera.main.transform.rotation;

            // Set position of seenMarker
            seenMarker.transform.position = RealToSeen(transform.position) + Camera.main.transform.position;

            // Indicate whether the camera's view of Player2D is obstructed
            if (playerStateManager.modeIs3D)
            {
                PushPlayer2DBack();

                if (IsViewClear())
                {
                    seenMarker.GetComponent<Renderer>().material = unobstructedView;
                }
                else
                {
                    seenMarker.GetComponent<Renderer>().material = obstructedView;
                }
            }

            // Set Player2D scale relative to its distance from the camera
            float realScale = SeenToReal(seenScale, transform.position);
            transform.localScale = new Vector3(realScale, realScale, realScale);

            // --- 2D MOVEMENT --- //
            if (!playerStateManager.modeIs3D && !isRespawning)
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
                    if (!isSlope)
                    {
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
                if (Input.GetButtonDown("Jump") && onFloor)
                {
                    velocityY = jumpStrength;
                    onFloor = false;

                    playerAudioSource.clip = jumpSound;
                    playerAudioSource.Play();

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
                    StartCoroutine(Respawn());
                }

                // --- GOAL LOGIC --- //
                // If Player2D touches the goal, return to hub world
                if (GetScaledCollision(seenMarker.transform.position) == "Goal")
                {
                    SceneManager.LoadScene("Hub", LoadSceneMode.Single);
                }
            }
        }
    }

    // Switch between 2D and 3D perspectives
    void SwitchPerspectives()
    {
        if (playerStateManager.modeIs3D) // If the player is switching from 3D to 2D
        {
            // Make sure Player2D is on screen
            if (IsPlayerOnScreen())
            {
                // Make sure the camera has an unobstructed view of Player2D
                if (IsViewClear())
                {
                    if (onFloor)
                    {
                        // Prevent the player from saving their jump status between perspective shifts
                        onFloor = false;
                    }

                    // Switch to 2D
                    playerStateManager.modeIs3D = false;
                }
                else
                {
                    StartCoroutine(playerStateManager.ShowWarning("Warning: 2D Avatar is obscured!"));
                }
            }
            else
            {
                StartCoroutine(playerStateManager.ShowWarning("Warning: 2D Avatar is not on-screen!"));
            }
        }
        else // If the player is switching from 2D to 3D
        {
            // If the player is in front of a level entry zone, enter the level
            if (FindSurfaceTypeAtPoint(transform.position) == "EntryZone")
            {
                EnterLevel();
                return;
            }

            // Push the player back to just in front of the wall they are on
            transform.position = FindClosestPoint(seenMarker.transform.position, lcBackwardInterval);
            anchorPos = GetOffsetPos(transform.position, -transform.up, seenScale / 2);

            // Save the player's current position in case they die
            lastSafePos = anchorPos;

            // Switch to 3D
            playerStateManager.modeIs3D = true;
        }

        // Reset Player2D's Y-velocity
        velocityY = 0.0f;

        playerStateManager.UpdateScreenBorderMaterial();
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
            // Get the position of Player2D offset in the local direction (player edges and corners)
            Vector3 offsetPos = GetOffsetPos(transform.position, localDir, seenScale / 2);

            Vector3 backDir = (offsetPos - Camera.main.transform.position).normalized;
            Vector3 pointToCheck = offsetPos - (backDir * transform.localScale.y);

            // If the player's view of Player2D is obstructed
            if (FindSurfaceTypeAtPoint(pointToCheck) != null)
            {
                // Add a degree of leniency to the view obstruction check
                if (FindSurfaceTypeAtPoint(pointToCheck) == "SurfaceMagenta" || localDir == Vector3.zero)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Push Player2D in front of the wall it is currently on
    void PushPlayer2DBack()
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

        // Reset Player2D position
        transform.position = anchorPos;
        Vector3 footPos = transform.position;
        transform.position = GetOffsetPos(footPos, transform.up, seenScale / 2);

        bool playerIsInsideWall = true;

        // While Player2D is inside of a wall
        while (playerIsInsideWall)
        {
            playerIsInsideWall = false;

            // For each local direction
            foreach (Vector3 localDir in localDirs)
            {
                // Get the position of Player2D offset in the local direction (player edges and corners),
                // as well as the position of Player2D offset in the negative of the local direction
                Vector3 offsetPosNorm = GetOffsetPos(transform.position, localDir, seenScale / 2);
                Vector3 offsetPosAnti = GetOffsetPos(transform.position, -localDir, seenScale / 2);
                Physics.Linecast(offsetPosNorm, offsetPosAnti, out RaycastHit hit);

                bool pointIsInsideWall = true;

                // While the current point is inside of a wall
                while (pointIsInsideWall)
                {
                    // If the current point is not inside of a wall, end the loop
                    if (hit.collider == null)
                    {
                        pointIsInsideWall = false;
                    }
                    else if (!surfaces.Contains(hit.collider.tag))
                    {
                        pointIsInsideWall = false;
                    }
                    else
                    {
                        // Player2D is still inside of a wall
                        playerIsInsideWall = true;

                        // Push Player2D closer to the camera
                        Vector3 backDir = (Camera.main.transform.position - footPos).normalized;

                        transform.position = footPos;
                        transform.Translate(backDir * Time.deltaTime * lcBackwardInterval, Space.World);
                        footPos = transform.position;
                        transform.position = GetOffsetPos(footPos, transform.up, seenScale / 2);

                        // Get new positions to check
                        offsetPosNorm = GetOffsetPos(transform.position, localDir, seenScale / 2);
                        offsetPosAnti = GetOffsetPos(transform.position, -localDir, seenScale / 2);
                        Physics.Linecast(offsetPosNorm, offsetPosAnti, out hit);
                    }
                }
            }
        }
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
            string sceneName = hit.collider.gameObject.GetComponent<EnterLevel>().sceneName;

            // Make sure the level exists before loading the level
            if (SceneManager.GetSceneByName(sceneName) == null)
            {
                Debug.Log($"Level \"{sceneName}\" does not exist.");
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
    }

    // Set Player2D's position in 3D space based on the SeenMarker's position in 2D space
    void ReSyncRealPosition()
    {
        transform.position = FindClosestPoint(seenMarker.transform.position);
    }

    // Get the 3D space equivalent of a value used by the SeenMarker
    float SeenToReal(float seenValue, Vector3 referencePoint)
    {
        float realCamDistance = Vector3.Magnitude(Camera.main.transform.position - referencePoint);
        return (seenValue / seenCamDistance) * realCamDistance;
    }

    // Convert a vector used by the Player2D's real position to a vector that the SeenMarker can use
    Vector3 RealToSeen(Vector3 realValue)
    {
        return Vector3.ClampMagnitude(realValue - Camera.main.transform.position, seenCamDistance);
    }

    private string FindSurfaceTypeAtPoint(Vector3 endPoint)
    {
        // Do a Linecast between the camera and the endPoint, then return the first GameObject hit by the Linecast
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

    // Do a Linecast from the camera in a direction until a surface is hit, then return the end position
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

        castPos += Vector3.ClampMagnitude(aimDir, lcBackwardInterval - pullBack);

        return castPos;
    }

    // Get a position offset in a direction
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

    // Get the highest priority surface colliding with the player
    string GetScaledCollision(Vector3 basePos)
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

        string[] collidingSurfaceTypes = new string[localDirs.Length];

        for (int i = 0; i < localDirs.Length; i++)
        {
            Vector3 localDir = localDirs[i];
            collidingSurfaceTypes[i] = FindOffsetSurfaceType(basePos, localDir, seenScale / 2);
        }

        string prioritizedSurfaceType = GetPrioritizedSurfaceType(collidingSurfaceTypes);

        return prioritizedSurfaceType;
    }

    // Take an array of surface types and return the one with the highest priority
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

    // Used for walking along sloped floors and sliding along sloped walls
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
    }

    // Removes the player's ability to jump after a set amount of time
    void DisableJump()
    {
        onFloor = false;
    }

    // Respawns the player
    IEnumerator Respawn()
    {
        isRespawning = true;

        // Play death sound
        playerAudioSource.clip = deathSound;
        playerAudioSource.Play();

        // Play death particle
        deathParticle.Play();

        // Hide Player2D
        seenMarker.GetComponent<Renderer>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        // Reset the player to the last position where they were safe
        anchorPos = lastSafePos;
        transform.position = GetOffsetPos(anchorPos, transform.up, seenScale / 2);

        seenMarker.transform.position = RealToSeen(transform.position) + Camera.main.transform.position;

        // Switch to 3D
        playerStateManager.modeIs3D = true;

        // Reset Player2D's Y-velocity
        velocityY = 0.0f;

        playerStateManager.UpdateScreenBorderMaterial();

        // Show Player2D
        seenMarker.GetComponent<Renderer>().enabled = true;

        isRespawning = false;
    }
}
