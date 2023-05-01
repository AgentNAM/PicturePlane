using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterLevel : MonoBehaviour
{
    // TODO: Implement level entry
    // public SceneAsset levelToLoad;

    public string levelName;
    private GameObject levelTitleText;

    // Start is called before the first frame update
    void Start()
    {
        levelTitleText = transform.parent.Find("EntryTop/LevelTitle").gameObject;
        levelTitleText.GetComponent<TextMesh>().text = levelName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
