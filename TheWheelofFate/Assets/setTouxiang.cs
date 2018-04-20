using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
public class Sc : MonoBehaviour
{

    [SerializeField]
    Image myImage;
    // Use this for initialization
    void Start()
    {
        string path1 = @"file:///C:\Users\pc\Pictures\timg.jpg";      //需要替换上去的资源路径
        StartCoroutine(test(path1));
    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator test(string path1)
    {

        //在C#中，需要用到yield的话，必须建立在IEnumerator类中执行。
        WWW www = new WWW(path1);
        //定义www为WWW类型并且等于所下载下来的WWW中内容。
        yield return www;
        //返回所下载的www的值

        if (string.IsNullOrEmpty(www.error))
        {
            Texture2D tex = www.texture;
            Sprite temp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            myImage.sprite = temp;
        }
        myImage.rectTransform.SetPositionAndRotation(new Vector3(0, 0, 0),new Quaternion(0F,0F,0F,0F));

    }
}
