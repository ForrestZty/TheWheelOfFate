using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour {

    public int healthPoint
    {
        get;
        private set;
    }
    private int manaPoint;

    private int level;
    private int attackPoint;
    private int defendPoint;
    private int speedPoint;

    private int actionPoint;

	// Use this for initialization
	void Start () {
        healthPoint = 100;
        manaPoint = 100;

        level = 1;
        attackPoint = 10;
        defendPoint = 10;
        speedPoint = 10;

        actionPoint = 10;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
