using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {

	/// <summary>
	/// This class will display the screen you see when you have lost the battle. This class needs to implement two buttons:
	/// One to allow the player to quit the battle and return to some other level you specify and another to reload this level.
	/// The launcher object should still be in the scene and will respawn the battle with all the original settings and values as before 
	/// creating the perfect mechanism for rematches
	/// </summary>
	public class tbbBattleLostScreen : MonoBehaviour {

		tbbBattleField battle_field { get { return tbbBattleField.Instance; } } 

		public mbsSlider
			pos;

		public int
			window_id = 78;

		public Texture2D
			rematch_img,
			quit_img;

		mbsStateMachineLeech<tbbeBattleState>
			state;

		public bool
			fix_display_size = true;
		
		bool
			gui_active = false;

		void Start()
		{
			state = new mbsStateMachineLeech<tbbeBattleState>(tbbBattleField.game_state);
			for( tbbeBattleState s = (tbbeBattleState)0; s < tbbeBattleState.Count; s++)
				state.AddState(s);

			pos.Init();
			pos.ForceState(eSlideState.Closed);
		}

		/// <summary>
		/// This is called after the "Defeat" message is shown.
		/// This script can display whatever you like but it needs to show two buttons to be useful:
		/// One to initiate a rematch and one to quit. Both functions are provided.
		/// </summary>
		virtual public void OnBattleFieldStateChangedToRetry()
		{
			gui_active = true;
			pos.Activate();
		}

		void OnGUI()
		{
			if (gui_active)
			{
				if (fix_display_size)
					GUIX.FixScreenSize();
				pos.FadeGUI();
				GUI.Window(window_id, pos.Pos, DrawRetry, ""); 
			}
		}

		void Update()
		{
			pos.Update();
		}

		/// <summary>
		/// This is what will actually be displayed during the retry screen. 
		/// This function is made virtual so you can override it when you subclass this component.
		/// This is displayed only if you lost the battle. This is where you would display the "Rematch" and "Quit" buttons.
		/// </summary>
		/// <param name="window_id">Window_id.</param>
		virtual public void DrawRetry(int window_id)
		{
			GUILayout.BeginArea(new Rect(50f, 50f, pos.targetPos.width-100, pos.targetPos.height-180));
			GUILayout.Label("Display whatever stats you want to display...");
			GUILayout.Label("Dish out some victory prizes and stats...");
			GUILayout.Label("This is the ideal place to do that...");

			if (GUILayout.Button(quit_img))
				GiveUpAndReturnToTheGame();
			
			if (GUILayout.Button(rematch_img))
				InitiateARematch();
			
			GUILayout.EndArea();
		}

		/// <summary>
		/// This function will load the level specified in level_to_load_on_victory inside tbbBattleField
		/// It will also destroy the launcher prefab that spawned the battle in the first place
		/// </summary>
		public void GiveUpAndReturnToTheGame()
		{
			tbbBattleLauncher launcher = GameObject.FindObjectOfType<tbbBattleLauncher>();
			if (null != launcher)
				Destroy(launcher.gameObject);
			
			Application.LoadLevel(tbbBattleField.Instance.level_to_load_on_quit);
		}

		/// <summary>
		/// This function will simply reload this level. This will trigger the launcher to respawn the battlefield and
		/// set everything back to their original settings, ready for a rematch
		/// </summary>
		public void InitiateARematch()
		{
			Application.LoadLevel(Application.loadedLevel);
		}

	}
}
