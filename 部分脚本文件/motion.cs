using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class motion : MonoBehaviour {

    public GameObject cubeNPC;      //能弹框的NPC
    public GameObject sphereSaber;  //主角

    // Use this for initialization
    void Start () {
        //自动绑定物体
        cubeNPC = GameObject.Find("CubeNPC");
        sphereSaber = GameObject.Find("SphereSaber");
	}
	
	// Update is called once per frame
	void Update () {
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
            sphereSaber.transform.localScale = new Vector3(1 + Mathf.Sin(Time.time), 1 + Mathf.Sin(Time.time), 1 + Mathf.Sin(Time.time));
        }
    }
}
