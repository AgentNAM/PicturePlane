using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Start a new game
    public void NewGame()
    {
        SceneManager.LoadSceneAsync("Hub");
    }

    // Exit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
