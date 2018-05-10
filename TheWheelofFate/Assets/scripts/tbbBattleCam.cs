using UnityEngine;
using System.Collections;

namespace MBS {

	[System.Serializable]
	/// <summary>
	/// Specifies a target camera position and location for multiple camera angles on the battlefield.
	/// </summary>
	public class tbbBattleCamTarget
	{
		/// <summary>
		/// This field should link to a disabled camera. The camera will never be used for anything
		/// more than position and rotation information so for all sense and purposes I could just 
		/// as well have used a Transform here instead of a Camera but by using a Camera is makes life
		/// just that little bit easier when position the transform as now you actually get to SEE what
		/// the final camera will "see" when it takes on this transform's properties
		/// </summary>
		public Camera 
			cam;		
	}

	/// <summary>
	/// This class handles the camera position during gameplay and allows you to toggle between multiple camera angles
	/// </summary>
	public class tbbBattleCam : MonoBehaviour {

		[SerializeField]
		/// <summary>
		/// If you leave this empty, the tbbBattleCam will automatically search for all Camera objects on your prefab
		/// and populate this field for you. If you want more control over what cameras will be used and in what order,
		/// simply specify them in this array in order of preference
		/// </summary>
		public tbbBattleCamTarget[]
			camera_angles;

		[System.NonSerialized]
		public int
			selected_cam_angle = 0;

		/// <summary>
		/// How quickly the camera should move from one angle to the next. This movement is dampened, meaning it will
		/// seem to have reached the target location quickly but is in fact only close to there and updating very slowly.
		/// As such, the speed here is not accurate as linear time and should be experimented with to find a proper value.
		/// 0.2 seems to give a smooth and quick transition and is the default
		/// </summary>
		public float
			cam_change_speed = 0.2f;

		/// <summary>
		/// Constantly checks to see if this key is pressed. Once it is pressed and the button released, the camera will
		/// move to the next available angle, if any. This value is case sensitive
		/// </summary>
		public string
			camera_toggle_key = "c";

		Camera 
			cam;
		
		Vector3 
			velocity = Vector3.zero,
			velocity2;

		bool 
			_ready;
	
		void OnBattleSystemInitialized () {
			ValidateBattleCamSetup();
		}
	
		void Update () {
			CheckForCameraToggle();
			TransitionCameraToTargetPos();
		}

		public void ValidateBattleCamSetup()
		{
			cam = Camera.main;
			if (!cam)
				cam = Camera.allCameras[0];

			if (null == camera_angles || camera_angles.Length == 0)
			{
				Camera[] cams = transform.GetComponentsInChildren<Camera>();

				if (null == cams)
				{
					StatusMessage.Message = "No camera angles defined!";
					return;
				} else
				{
					camera_angles = new tbbBattleCamTarget[cams.Length];
					for(int i = 0; i < cams.Length; i++)
					{
						camera_angles[i] = new tbbBattleCamTarget();
						camera_angles[i].cam = cams[i];
					}
				}
			}
			_ready = true;
		}
	
		void CheckForCameraToggle()
		{
			if ( Input.GetKeyUp(camera_toggle_key) && null != camera_angles)
			{
				if (camera_angles.Length > 1)
				{
					selected_cam_angle = (selected_cam_angle >= camera_angles.Length - 1) ? 0 : selected_cam_angle+1;
					velocity = velocity2 = Vector3.zero;
				}
			}
		}

		void TransitionCameraToTargetPos()
		{
			if (!_ready)
			{
				return;
			}

			if (cam.transform.position != camera_angles[selected_cam_angle].cam.transform.position)
			{
				Vector3 target_pos = camera_angles[selected_cam_angle].cam.transform.position;
				cam.transform.position = Vector3.SmoothDamp(cam.transform.position, target_pos, ref velocity, cam_change_speed);
			}

			if (cam.transform.forward != camera_angles[selected_cam_angle].cam.transform.forward)
			{
				Vector3 target_dir = camera_angles[selected_cam_angle].cam.transform.forward;
				cam.transform.forward = Vector3.SmoothDamp(cam.transform.forward, target_dir, ref velocity2, cam_change_speed);
			}
		}

	}
}
