﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {

    public GameObject cubeNPC;      //能弹框的NPC
    public GameObject sphereSaber;  //主角
    public GameObject dialog;       //弹框                                
    public GameObject button;       //弹框按键

    // Use this for initialization
    void Start()
    {
        //自动绑定物体
        cubeNPC = GameObject.Find("CubeNPC");
        sphereSaber = GameObject.Find("Edna");
        dialog = GameObject.Find("Dialog");
        button = GameObject.Find("Button");
        Button btn = (Button)button.GetComponent<Button>(); //获取按钮脚本组件
        dialog.SetActive(false);                            //让弹框先隐藏起来
    }

    // Update is called once per frame
    void Update()
    {
        //如果cubeNPC或者sphereSaber实例化失败就跳出函数  
        if (cubeNPC == null || sphereSaber == null)
        {
            Debug.Log("实例化失败");
            return;
        }

        //计算出NPC和主角之间的距离
        float dis = Vector3.Distance(cubeNPC.transform.position, sphereSaber.transform.position);

        //当主角与NPC小于一定距离时，可以通过按键触发弹框
        if (dis < 10)
        {
            //sphereSaber.transform.localScale = new Vector3(1 + Mathf.Sin(Time.time), 1 + Mathf.Sin(Time.time), 1 + Mathf.Sin(Time.time));
            //监听键盘按下空格事件（如果使用空格作为主角与NPC的交互按键）
            //弹框
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Debug.Log("按下了空格");
                dialog.SetActive(true);
            }
        }

    }

    //此函数接口将赋予给按钮的onClick事件  
    public void onOK()
    {
        dialog.SetActive(false);//让自己隐藏  
    }
}
