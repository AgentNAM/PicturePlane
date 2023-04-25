using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterLevel : MonoBehaviour
{
    // TODO: Implement level entry
    public string levelName;

    private Player2DController player2DControllerScript;

    // Start is called before the first frame update
    void Start()
    {
        player2DControllerScript = GameObject.Find("Player2DRealMarker").GetComponent<Player2DController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
