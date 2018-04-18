using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OPScript : MonoBehaviour {

    // Use this for initialization
    public GameObject im;
    public VideoPlayer vp;
    private AsyncOperation operation;
    void Start () {
        vp.loopPointReached += EndReadched;
        vp.prepareCompleted += Prepared;
        vp.Prepare();
        StartCoroutine(AsyncLoading());
    }

    IEnumerator AsyncLoading()
    {
        operation = SceneManager.LoadSceneAsync("mainMenu");

        //阻止当加载完成自动切换
        operation.allowSceneActivation = false;

        yield return operation;
    }
    // Update is called once per frame
    void Update () {
        
    }
    void Prepared(VideoPlayer vp)
    {
        im.SetActive(false);
        vp.Play();
    }

    void EndReadched(VideoPlayer vp)
    {
        operation.allowSceneActivation = true;
    }
    
}
