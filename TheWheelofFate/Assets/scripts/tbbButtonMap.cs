using UnityEngine;
using System.Collections;

namespace MBS {
	
	/// <summary>
	/// The default Vita Key Mappings. You can access them directly by doing something like:
	///     if(tbbVitaInput.cross_button_up) DoSomething();
	/// This will give you Vita only controls. Alternatively, see the tbbButtonMap class for a cross platform solution
	/// </summary>
	static public class tbbVitaInput
	{
		static public bool cross_button_up				{ get { return Input.GetKeyUp(KeyCode.JoystickButton0); } }
		static public bool circle_button_up 			{ get { return Input.GetKeyUp(KeyCode.JoystickButton1); } }
		static public bool square_button_up 			{ get { return Input.GetKeyUp(KeyCode.JoystickButton2); } }
		static public bool triangle_button_up 			{ get { return Input.GetKeyUp(KeyCode.JoystickButton3); } }
		static public bool left_shoulder_button_up		{ get { return Input.GetKeyUp(KeyCode.JoystickButton4); } }
		static public bool right_shoulder_button_up 	{ get { return Input.GetKeyUp(KeyCode.JoystickButton5); } }
		static public bool select_button_up 			{ get { return Input.GetKeyUp(KeyCode.JoystickButton6); } }
		static public bool start_button_up 				{ get { return Input.GetKeyUp(KeyCode.JoystickButton7); } }
		static public bool up_button_up 				{ get { return Input.GetKeyUp(KeyCode.JoystickButton8); } }
		static public bool right_button_up 				{ get { return Input.GetKeyUp(KeyCode.JoystickButton9); } }
		static public bool down_button_up 				{ get { return Input.GetKeyUp(KeyCode.JoystickButton10); } }
		static public bool left_button_up 				{ get { return Input.GetKeyUp(KeyCode.JoystickButton11); } }
		
		static public bool cross_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton0); } }
		static public bool circle_button_down			{ get { return Input.GetKeyDown(KeyCode.JoystickButton1); } }
		static public bool square_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton2); } }
		static public bool triangle_button_down 		{ get { return Input.GetKeyDown(KeyCode.JoystickButton3); } }
		static public bool left_shoulder_button_down	{ get { return Input.GetKeyDown(KeyCode.JoystickButton4); } }
		static public bool right_shoulder_button_down	{ get { return Input.GetKeyDown(KeyCode.JoystickButton5); } }
		static public bool select_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton6); } }
		static public bool start_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton7); } }
		static public bool up_button_down 				{ get { return Input.GetKeyDown(KeyCode.JoystickButton8); } }
		static public bool right_button_down			{ get { return Input.GetKeyDown(KeyCode.JoystickButton9); } }
		static public bool down_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton10); } }
		static public bool left_button_down 			{ get { return Input.GetKeyDown(KeyCode.JoystickButton11); } }
		
		static public bool cross_button 				{ get { return Input.GetKey(KeyCode.JoystickButton0); } }
		static public bool circle_button				{ get { return Input.GetKey(KeyCode.JoystickButton1); } }
		static public bool square_button 				{ get { return Input.GetKey(KeyCode.JoystickButton2); } }
		static public bool triangle_button				{ get { return Input.GetKey(KeyCode.JoystickButton3); } }
		static public bool left_shoulder_button			{ get { return Input.GetKey(KeyCode.JoystickButton4); } }
		static public bool right_shoulder_button		{ get { return Input.GetKey(KeyCode.JoystickButton5); } }
		static public bool select_button				{ get { return Input.GetKey(KeyCode.JoystickButton6); } }
		static public bool start_button					{ get { return Input.GetKey(KeyCode.JoystickButton7); } }
		static public bool up_button					{ get { return Input.GetKey(KeyCode.JoystickButton8); } }
		static public bool right_button					{ get { return Input.GetKey(KeyCode.JoystickButton9); } }
		static public bool down_button					{ get { return Input.GetKey(KeyCode.JoystickButton10); } }
		static public bool left_button					{ get { return Input.GetKey(KeyCode.JoystickButton11); } }

		static public float left_horizontal				{ get { return Input.GetAxis("VitaLeftHorizontal");	} }
		static public float left_vertical				{ get { return Input.GetAxis("VitaLeftVertical");	} }
		static public float right_horizontal			{ get { return Input.GetAxis("VitaRightHorizontal");} }
		static public float right_vertical				{ get { return Input.GetAxis("VitaRightVertical");	} }
		static public float left_shoulder				{ get { return Input.GetAxis("VitaLeftShoulder");	} }
		static public float right_shoulder				{ get { return Input.GetAxis("VitaRightShoulder");	} }
	}

	/// <summary>
	/// This is my cross platform input solution. Instead of testing for individual key presses right throughout my
	/// project, I instead choose to define "Buttons", give them names and test for when my "Buttons" were pressed.
	/// 
	/// For instance, I would say that I need a jump button in my game so I create a jump button in here. 
	/// I simply do my input checking here for all keys that I will accept for jumping so that means I check the keyboard
	/// as well the Vita buttons and simply return a true or false.
	/// 
	/// It is not some extrodinary piece of coding engineering or anything spectacular like that but it does have the benefit
	/// of allowing me to check for keyboard and Vita input by doing something as simple as this:
	///     if (tbbButtonMap.jump_button_up) DoJump();
	/// 
	/// Modify this class to suit your game's needs...
	/// </summary>
	static public class tbbButtonMap
	{
		static public bool left_button_up { get { return Input.GetKeyUp(KeyCode.LeftArrow) || tbbVitaInput.left_button_up ; } }
		static public bool right_button_up { get { return Input.GetKeyUp(KeyCode.RightArrow) || tbbVitaInput.right_button_up ; } }
		static public bool up_button_up { get { return Input.GetKeyUp(KeyCode.UpArrow) || tbbVitaInput.up_button_up ; } }
		static public bool down_button_up { get { return Input.GetKeyUp(KeyCode.DownArrow) || tbbVitaInput.down_button_up ; } }

		static public bool camera_button_up { get { return Input.GetKeyUp(KeyCode.C) || tbbVitaInput.square_button_up ; } }
		static public bool action_button_up { get { return Input.GetKeyUp(KeyCode.Return) || tbbVitaInput.cross_button_up ; } }
		static public bool cancel_button_up { get { return Input.GetKeyUp(KeyCode.Escape) || tbbVitaInput.circle_button_up ; } }
	}

}
