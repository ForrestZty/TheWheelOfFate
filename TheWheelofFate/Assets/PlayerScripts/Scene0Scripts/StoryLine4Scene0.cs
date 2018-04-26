using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class StoryLine4Scene0 : MonoBehaviour {

    public GameObject trigger1;
    public GameObject trigger2;
    public GameObject trigger3;

    private GameObject soundbox;

    
    public VideoPlayer movie1, movie2;    
	// Use this for initialization
	void Start () {
        soundbox = GameObject.Find("soundbox");
        soundbox.gameObject.SetActive(false);
        trigger2.SetActive(false);
        trigger3.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown)
        {
            if (movie1.isPlaying==true)
            {
                movie1.Stop();
                movie1.gameObject.SetActive(false);
                Globe.canBeControlled = true;
            }
            if (movie2.isPlaying == true)
            {
                movie2.Stop();
                movie2.gameObject.SetActive(false);
                Globe.canBeControlled = true;
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Trigger1")
        {
            trigger1On();
        }
        if (other.name == "Trigger2")
        {
            trigger2On();
        }
        if (other.name == "Trigger3")
        {
            trigger3On();
        }
    }

    void trigger1On()
    {
        movie1.loopPointReached += EndReadched;
        Globe.canBeControlled = false;
        trigger1.gameObject.SetActive(false);
        movie1.Play();
        trigger2.gameObject.SetActive(true);
    }
    void trigger2On()
    {
        movie2.loopPointReached += EndReadched;
        Globe.canBeControlled = false;
        trigger2.gameObject.SetActive(false);
        movie2.Play();
        trigger3.gameObject.SetActive(true);
    }
    void trigger3On()
    {
        Globe.nextSceneName = "Scene1";
        SceneManager.LoadScene("loadingScene");
        
    }
    void EndReadched(VideoPlayer vp)
    {
        vp.gameObject.SetActive(false);
        Globe.canBeControlled = true;
    }
  
}
