using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToScene1 : MonoBehaviour {

    public string sceneName;
    public GameObject soundbox;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(soundbox);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadSceneOne()
    {
        Globe.nextSceneName = sceneName;
        SceneManager.LoadScene("loadingScene");
    }
    
}
