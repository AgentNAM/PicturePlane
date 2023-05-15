using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerStateManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject screenBorderHolder;
    public TextMeshProUGUI warningText;

    public Material borderMaterial3D;
    public Material borderMaterial2D;

    public bool modeIs3D = true;
    public bool paused = false;

    private float warningTime = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        ResumeGame();
    }

    // Update is called once per frame
    void Update()
    {
        // Pause game when player presses esc
        if (Input.GetButtonDown("Cancel"))
        {
            if (!paused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    // Change screen border transparency depending on current perspective mode
    public void UpdateScreenBorderMaterial()
    {
        Transform[] screenBorders = Array.FindAll(screenBorderHolder.GetComponentsInChildren<Transform>(), child => child != screenBorderHolder.transform);

        foreach (Transform screenBorder in screenBorders)
        {
            if (modeIs3D)
            {
                screenBorder.gameObject.GetComponent<Renderer>().material = borderMaterial3D;
            }
            else
            {
                screenBorder.gameObject.GetComponent<Renderer>().material = borderMaterial2D;
            }
        }
    }

    // Show warning (if the player tries to switch perspectives while the 2D avatar is off-screen or obstructed)
    public IEnumerator ShowWarning(string message)
    {
        warningText.gameObject.SetActive(true);
        warningText.text = message;

        yield return new WaitForSeconds(warningTime);

        warningText.gameObject.SetActive(false);
    }

    // Pause game
    void PauseGame()
    {
        paused = true;

        pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Resume game
    public void ResumeGame()
    {
        paused = false;

        pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Restart level
    public void RestartLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    // Back to hub
    public void BackToHub()
    {
        SceneManager.LoadSceneAsync("Hub");
    }

    // Back to main menu
    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync("Title Screen");
    }

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
