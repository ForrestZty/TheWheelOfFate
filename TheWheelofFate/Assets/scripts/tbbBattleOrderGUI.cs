using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	///
	/// </summary>
	public class tbbBattleOrderGUI : MonoBehaviour {

		/// <summary>
		/// Should these control be scaled to fit any screen size or should the values be absolute
		/// </summary>
		public bool 	fixed_screen_area = true;
		/// <summary>
		/// For Human vs AI matches this field is ignored
		/// For two player games this should be set to false
		///
		/// </summary>
		public bool 	show_only_my_faction = true;

		public float
			screen_width = 1024f,
			screen_height = 768f;

		public mbsSlider
			battle_order_area;

		public Rect[]
			battle_order_areas;

		public Rect
			avatar_area = new Rect(5,5,108,108),
			hp_bar_area,
			mp_bar_area,
			healthbar_offset;

		public Texture2D
			health_bar_bg,
			health_bar_img;

		public Color
			HP_color = Color.green,
			MP_color = Color.blue;

		public GUISkin
			battle_skin;

		tbbBattleField battle_field { get { return tbbBattleField.Instance ; }}
		public float FontSize 		{ get { float result = battle_skin == null ? 25f : (battle_skin.label.fontSize + 5f); return result == 0  ? 25f : result;} } 

		void Start()
		{
			battle_order_area.Init();
			battle_order_area.ForceState(eSlideState.Closed);

			tbbBattleField.active_faction.onFactionTurnDone += TestForGUIHide;
			tbbBattleField.opponent.onFactionTurnDone += TestForGUIHide;
		}

		void TestForGUIHide(tbbFaction faction)
		{
			OnCharSelectStateChanged(tbbeCharSelModes.Count);
		}

		void OnCharacterSelectInitialized()
		{
			//configure screen size
			if (fixed_screen_area)
				GUIX.SetScreenSize(screen_width, screen_height);
		}

		void Update()
		{
			battle_order_area.Update();
		}

		public void OnCharSelectStateChanged(tbbeCharSelModes state)
		{
			if (null == tbbBattleField.active_faction.battle_order || tbbBattleField.active_faction.Intelligence == tbbeControlMethod.AI || tbbBattleField.game_state.CurrentState  >= tbbeBattleState.Battling)
			{
				if (!battle_order_area.slideState.CompareState(eSlideState.Closed))
					battle_order_area.Deactivate();
			} else
			{
				if (((	tbbBattleField.active_faction.battle_order.Count == 0 
				     || tbbBattleField.game_state.CurrentState  >= tbbeBattleState.Battling)
				     || ( show_only_my_faction &&  tbbBattleField.active_faction != tbbBattleField.me )
				     )
				    && !battle_order_area.slideState.CompareState(eSlideState.Closed) )
				{
					battle_order_area.Deactivate();
				}
				else
				{
					if (tbbBattleField.active_faction.battle_order.Count > 0)
					{
						if ( battle_order_area.slideState.CompareState(eSlideState.Closed))
						{
							if ((show_only_my_faction && tbbBattleField.active_faction == tbbBattleField.me)
							    || tbbBattleField.active_faction.Intelligence == tbbeControlMethod.Human)
								battle_order_area.Activate();
						}
					}
				}
			}
		}

		void OnGUI()
		{
			if (battle_order_area.slideState.CompareState(eSlideState.Closed))
				return;
			
			GUI.skin = battle_skin;

			battle_order_area.FadeGUI();
			GUI.BeginGroup(battle_order_area.Pos);
			int index = 0;
			if(null != tbbBattleField.active_faction.battle_order)
			foreach(tbbBattleOrder turn in tbbBattleField.active_faction.battle_order)
			{
				if  (index >= battle_order_areas.Length)
					break;
				
				DisplayPlayerDetails( turn.character, battle_order_areas[index] );
				index++;
			}
			GUI.EndGroup();
			battle_order_area.FadeGUI(false);
		}

		void DisplayPlayerDetails(tbbPlayerInfo character, Rect area)
		{
			GUI.Box (area, "");
			GUI.BeginGroup(area);
			
			if (null != character.avatar)
				GUI.DrawTexture(avatar_area, character.avatar);

			GUI.EndGroup ();
		}

	}
}
