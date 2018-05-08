using UnityEngine;
using System.Collections;

namespace MBS {
	/// <summary>
	/// This class represents an animated graphic used during intro and turn switching
	/// </summary>
	public class tbbIntroText : MonoBehaviour {
		
		mbsStateMachine<tbbeIntroStates>
			state;
		
		/// <summary>
		/// The position to display this graphic on screen. Also the speed and slide direction.
		/// </summary>
		public mbsSlider
			area;
		
		/// <summary>
		/// How long will this display before sliding out
		/// </summary>
		public float 
			display_time = 4f;
		
		/// <summary>
		/// The graphic you want to display in this area
		/// </summary>
		public Texture2D 
			msg_icon;
		
		
		/// <summary>
		/// Triggers when the image starts to animate into view
		/// </summary>
		public System.Action	onStarting;
		/// <summary>
		/// Triggers when the content has reached it's target screen position
		/// </summary>
		public System.Action	onDisplaying;
		/// <summary>
		/// Triggers when the content starts to move out of view
		/// </summary>
		public System.Action	onClosing;
		/// <summary>
		/// Triggers when the content is done moving out of view
		/// </summary>
		public System.Action	onDone;
		
		
		void Awake()
		{
			area.Init();
			area.ForceState(eSlideState.Closed);
			
			area.OnActivating	= _onStarting;
			area.OnActivated	= _onDisplaying;
			area.OnDeactivated	= _onDone;
			
			state = new mbsStateMachine<tbbeIntroStates>();
			state.AddState(tbbeIntroStates.Waiting);
			state.AddState(tbbeIntroStates.Starting		, ShowMessage);
			state.AddState(tbbeIntroStates.Displaying	, ShowMessage);
			state.AddState(tbbeIntroStates.Closing		, ShowMessage);
			state.AddState(tbbeIntroStates.Done);
			
			state.SetState(tbbeIntroStates.Waiting);
		}
		
		void Update () {
			area.Update();
		}
		
		void OnGUI()
		{
			state.PerformAction();
		}
		
		void ShowMessage()
		{
			GUIX.FixScreenSize();
			area.FadeGUI();
			GUI.Label (area.Pos, msg_icon);
			area.FadeGUI(false);
		}
		
		/// <summary>
		/// Starts the animation
		/// </summary>
		public void StartIntro()
		{
			area.Activate();
		}
		
		void _onStarting()
		{
			state.SetState(tbbeIntroStates.Starting);
			if (null != onStarting)
				onStarting();
		}
		
		void _onDisplaying()
		{
			state.SetState(tbbeIntroStates.Displaying);
			if (null != onDisplaying)
				onDisplaying();
			
			Invoke("_onClosing", display_time);
		}
		
		void _onClosing()
		{
			area.Deactivate();
			state.SetState(tbbeIntroStates.Closing);
			if (null != onClosing)
				onClosing();
		}
		
		void _onDone()
		{
			state.SetState(tbbeIntroStates.Waiting);
			if (null != onDone)
				onDone();
		}
	}
}
