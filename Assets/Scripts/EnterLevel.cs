using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnterLevel : MonoBehaviour
{
    // TODO: Implement level entry
    public SceneAsset levelToLoad;

    private GameObject levelTitle;

    // Start is called before the first frame update
    void Start()
    {
        levelTitle = transform.parent.Find("EntryTop/LevelTitle").gameObject;
        levelTitle.GetComponent<TextMesh>().text = levelToLoad.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
