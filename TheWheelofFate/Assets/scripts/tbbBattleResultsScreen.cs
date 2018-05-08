using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// This class handles the display of the mission results screen and is only displayed if and when the battle was won by you
	/// </summary>
	public class tbbBattleResultsScreen : MonoBehaviour {

		tbbBattleField battle_field { get { return tbbBattleField.Instance; } } 

		public mbsSlider
			pos;

		public int
			window_id = 79;

		public Texture2D
			continue_img;

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
		/// This is called after the "Victory" message is shown.
		/// This is the ideal place to do your XP and item awards unless you want to create a standalone
		/// script to do that. If you do want to create a standalone script for that I would recommend
		/// subclassing it from this one and just override this function so the rest still stays in tact.
		/// </summary>
		virtual public void OnBattleFieldStateChangedToResultsScreen()
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
				GUI.Window(window_id, pos.Pos, DrawResults, ""); 
			}
		}

		void Update()
		{
			pos.Update();
		}

		/// <summary>
		/// This is what will actually be displayed during the mission results screen. 
		/// This function is made virtual so you can override it when you subclass this component.
		/// This is displayed only if you won the battle. So there needs to be a button somewhere here
		/// that loads the next level...
		/// </summary>
		/// <param name="window_id">Window_id.</param>
		virtual public void DrawResults(int window_id)
		{
			GUILayout.BeginArea(new Rect(50f, 50f, pos.targetPos.width-100, pos.targetPos.height-100));
			GUILayout.Label("Display whatever stats you want to display...");
			GUILayout.Label("Dish out some victory prizes and stats...");
			GUILayout.Label("This is the ideal place to do that...");

			if (GUILayout.Button(continue_img))
				ReturnToGame();

			GUILayout.EndArea();
		}

		/// <summary>
		/// This function will load the level specified in level_to_load_on_victory inside tbbBattleField
		/// It will also destroy the launcher prefab that spawned the battle in the first place
		/// </summary>
		public void ReturnToGame()
		{
			tbbBattleLauncher launcher = GameObject.FindObjectOfType<tbbBattleLauncher>();
			if (null != launcher)
				Destroy(launcher.gameObject);

			Application.LoadLevel(tbbBattleField.Instance.level_to_load_on_victory);
		}

	}
}
