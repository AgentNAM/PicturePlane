using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EnterLevel : MonoBehaviour
{
    public string sceneName;
    public string levelName;
    private GameObject levelTitle;

    // Start is called before the first frame update
    void Start()
    {
        // Set level title text
        levelTitle = transform.parent.Find("EntryTop/LevelTitle").gameObject;
        levelTitle.GetComponent<TextMeshPro>().text = levelName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
