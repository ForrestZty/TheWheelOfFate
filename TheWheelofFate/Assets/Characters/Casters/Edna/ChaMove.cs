using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChaMove : MonoBehaviour {
    private CharacterController cha; // control character
    private Animator ani; // contorl animator
    private bool Xie = false; // if the character

    public Camera Cam;
    public float m_fspeed_r;
	public float m_fspeed_w;
	private float r_fspeed = 0f;
	public float walk_acceleration;
	public float run_acceleration;
    public float stop_acceleration;

    

	// Use this for initialization
	void Start () {
        cha = this.GetComponent<CharacterController>();
        ani = this.GetComponent<Animator>();

        
        
        ani.SetFloat("speed", 0.0f);
    }

	// judge if shift put down
	bool ifrun() {
		if (Input.GetKey (KeyCode.LeftShift))
			return true;
		else
			return false;
	}

    // judge if input key WASD
    bool ifmove()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            return true;
        else
            return false;
    }

	// the direction character move
	Vector3 vecCalculate()
    {
      

        Vector3 result = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            //y += 1;
            result += Cam.transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            //y -= 1;
            result += -Cam.transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //x -= 1;
            result += -Cam.transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            //x += 1;
            result += Cam.transform.right;
        }
        
        if (result.x != 0 && result.z != 0)
            Xie = true;
        else
            Xie = false;

        result.y = 0;
        //Vector3 result = new Vector3(x,z,y);
        return result;
    }

    void Rotating(Vector3 vec)
    {
        Quaternion quaDir = Quaternion.LookRotation(vec, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation,quaDir,Time.deltaTime * 10f);
    }

	// Update is called once per frame
	void Update () {
        if (Globe.canBeControlled == true) { 
        Vector3 mForward = vecCalculate();
        Vector3 zero = new Vector3(0, 0, 0);
        if (!cha.isGrounded)
        {
            cha.Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
            //cc.Move(new Vector3(0, m_gspeed * Time.deltaTime, 0));// 
            //m_gspeed -= gravity;
        }

        // 1.2 moving
        // running
        if (ifrun()) {
			
			//m_fspeed = m_fspeed + (run_acceleration * Time.deltaTime);
			r_fspeed += run_acceleration * Time.deltaTime;

			if (Xie && r_fspeed <= m_fspeed_r)
				cha.Move (mForward * Time.deltaTime * r_fspeed / 1.41f);
			else if (Xie && r_fspeed > m_fspeed_r)
            {
                r_fspeed = m_fspeed_r;
                cha.Move (mForward * Time.deltaTime * r_fspeed / 1.41f);
            }
				
			else if (r_fspeed <= m_fspeed_r)
				cha.Move (mForward * Time.deltaTime * r_fspeed);
            else
            {
                r_fspeed = m_fspeed_r;
                cha.Move (mForward * Time.deltaTime * r_fspeed);
            }
				
				
		}
        // walking
        else {
            r_fspeed += walk_acceleration * Time.deltaTime;

            if (Xie && r_fspeed <= m_fspeed_w)
                cha.Move(mForward * Time.deltaTime * r_fspeed / 1.41f); 
            else if (Xie && r_fspeed > m_fspeed_w)
            {
                r_fspeed = m_fspeed_w;
                cha.Move(mForward * Time.deltaTime * m_fspeed_w / 1.41f);
            }
                
            else if (r_fspeed <= m_fspeed_w)
                cha.Move(mForward * Time.deltaTime * r_fspeed);
            else
            {
                r_fspeed = m_fspeed_w;
                cha.Move(mForward * Time.deltaTime * r_fspeed);
            }
                
		}

        if (mForward != zero)
        {
            ani.SetFloat("speed", r_fspeed); // paly the animater
            if (this.transform.forward != mForward)
            {
                Rotating(mForward);
            }
        }
        else ani.SetFloat("speed", 0.0f);

        // stop
        if (!ifmove() && r_fspeed != 0f)
        {
            r_fspeed = r_fspeed - stop_acceleration;
            if (r_fspeed <= 0f)
                r_fspeed = 0f;
        }
        // run to walk
        else if(ifmove() && !ifrun())
        {
            r_fspeed = m_fspeed_w;
        }
        }
    }
}
