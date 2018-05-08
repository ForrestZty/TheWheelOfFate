using UnityEngine;
using System.Collections;

namespace MBS{
	/// <summary>
	/// This sets up a separate camera for the battle and sets it to only render the TextOverlays layer
	/// then forces it to be a child of the MainCamera.
	/// This class was created to simplify the display of the text that floats up when a character takes damage
	/// and it's sole purpose is to automate the setup. As such, after placing this component on the object,
	/// there is nothing more for you to do in this component. Drop it on the BattleField and forget about it...
	/// </summary>
	public class tbbDamageCam : MonoBehaviour {
		void Start () {
			GameObject go = new GameObject("Damage Cam");
			Camera damage_cam = go.AddComponent<Camera>();
			damage_cam.gameObject.layer = LayerMask.NameToLayer("TextOverlays");
			damage_cam.cullingMask = 1 << LayerMask.NameToLayer("TextOverlays");

			damage_cam.transform.position = Camera.main.transform.position;
			damage_cam.transform.rotation = Camera.main.transform.rotation;
			damage_cam.transform.parent = Camera.main.transform;
			damage_cam.depth = 1;
			damage_cam.clearFlags = CameraClearFlags.Depth;
		}		
	}
}
