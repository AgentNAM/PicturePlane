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
        if (Input.GetKeyDown(KeyCode.Q))
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

    public IEnumerator ShowWarning(string message)
    {
        warningText.gameObject.SetActive(true);
        warningText.text = message;

        yield return new WaitForSeconds(warningTime);

        warningText.gameObject.SetActive(false);
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

    void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        paused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseMenu.SetActive(false);

        paused = false;
    }

    public void RestartLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void BackToHub()
    {
        SceneManager.LoadSceneAsync("Hub");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync("Title Screen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
