using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// This is one possible GUI for the character selection part of the system.
	/// If you do not like the current way the character selection is done,
	/// simply modify or replace this component with the code you WANT to execute
	/// </summary>
	[RequireComponent ( typeof (tbbCharSelect ) ) ]
	public class tbbCharSelectGUI : MonoBehaviour {
		
		tbbCharSelect char_select { get { return tbbCharSelect.Instance; }}
		
		/// <summary>
		/// This probes the current state of the character select system so the gui
		/// can display relevant info as required by the character select state
		/// </summary>
		public mbsStateMachineLeech<tbbeCharSelModes> 
			gui_state;
		
		public bool 
			/// <summary>
			/// In case you elect to make the GUI stretch to fit all display sizes, set this to true
			/// </summary>
			fixed_screen_area = true,
			
			/// <summary>
			/// Should the MP this spell requires be shown next to the name in the spell selection panel?
			/// </summary>
			show_mp;
		
		
		/// <summary>
		/// if <c>fixed_screen_size</c> is true, this is the virtual width of the screen to
		/// use when doing gui element layout
		/// </summary>
		public float	screen_width = 1024f;
		
		/// <summary>
		/// if <c>fixed_screen_size</c> is true, this is the virtual height of the screen to
		/// use when doing gui element layout
		/// </summary>
		public float	screen_height = 768f;
		
		
		/// <summary>
		/// The total area of the screen to use to display the player's party members in.
		/// This area is animated and all content will be clipped by the values set here
		/// </summary>
		public mbsSlider	player_area;
		
		/// <summary>
		/// The total area of the screen to use to display the enemy's party members in.
		/// This area is animated and all content will be clipped by the values set here
		/// </summary>
		public mbsSlider	enemy_area;
		
		
		/// <summary>
		/// The color to display the health bar in
		/// </summary>
		public Color	HP_color = Color.green;
		
		/// <summary>
		/// The color to display the mana bar in
		/// </summary>
		public Color	MP_color = Color.blue;
		
		/// <summary>
		/// The gui used by the elements of this component
		/// </summary>
		public GUISkin
			battle_skin;
		
		/// <summary>
		/// An image to display behind the health and mana bars as a frame around it.
		/// Be sure to include what is meant to display where the power is lost
		/// </summary>
		public Texture2D	health_bar_bg;
		
		/// <summary>
		/// This should just be a plain white image if you want to make use of tinting.
		/// Alternatively, this image should span the entire width and height of the
		/// area allocated for the health and will be clipped based on the stat value
		/// </summary>
		public Texture2D	health_bar_img;
		
		
		/// <summary>
		/// For each element of the individual character display area, the offset and
		/// size to use for drawing the avatar image
		/// </summary>
		public Rect	avatar_area = new Rect(5,5,108,108);
		
		/// <summary>
		/// For each element of the individual character display area, the offset and
		/// size to use for drawing the health bar
		/// </summary>
		public Rect	hp_bar_area;
		
		/// <summary>
		/// For each element of the individual character display area, the offset and
		/// size to use for drawing the mana bar
		/// </summary>
		public Rect	mp_bar_area;
		
		/// <summary>
		/// For each element of the individual character display area, the offset and
		/// size into the health and mana bars to use for drawing the actual energy value
		/// </summary>
		public Rect	healthbar_offset;
		
		
		/// <summary>
		/// Define the actual, individual areas where each character will be displayed
		/// within the <c>player_area</c>
		/// </summary>
		public Rect[]	player_info_areas;
		
		/// <summary>
		/// Define the positions in which to show the available actions a character has
		/// </summary>
		public Rect[]	player_action_areas;
		
		/// <summary>
		/// Define the position in which to show the available choices under an action
		/// </summary>
		public Rect[]	player_action_submenu_areas;
		
		/// <summary>
		/// Define the actual, individual areas where each enemy will be displayed
		/// within the <c>enemy_area</c>
		/// </summary>
		public Rect[]	enemy_info_areas;
		
		public float FontSize 		{ get { float result = battle_skin == null ? 25f : (battle_skin.label.fontSize + 5f); return result == 0  ? 25f : result;} } 
		
		bool
			_ready = false;
		
		tbbBattleOrder LastEntry	{ get { return tbbBattleField.active_faction.LastBattleOrderEntry;}}
		int SelectedChar			{ get { return char_select.selected_char;	} set { char_select.selected_char	= value;} }
		int SelectedAction			{ get { return LastEntry.character.selected_action; } set { LastEntry.character.selected_action = value;} }
		int SelectedAttack			{ get { return char_select.selected_attack; } 
			set { char_select.selected_attack = value;} }
		int SelectedTarget			{ get { return char_select.selected_target; } set { char_select.selected_target = value;} }
		
		void OnBattleSystemInitialized()
		{
			enemy_area.Init();
			enemy_area.ForceState(eSlideState.Closed);
			
			player_area.Init();
			player_area.ForceState(eSlideState.Closed);
			
			_ready = true;
		}
		
		void OnCharacterSelectInitialized()
		{
			gui_state = new mbsStateMachineLeech<tbbeCharSelModes>(char_select.char_sel_state);
			gui_state.AddState(tbbeCharSelModes.CharacterSelect		   );
			gui_state.AddState(tbbeCharSelModes.ActionSelect			,	DisplayActionSelect);
			gui_state.AddState(tbbeCharSelModes.ActionRefinementSelect	,	DisplayActionRefinementSelect);
			gui_state.AddState(tbbeCharSelModes.SpellSelect				,	DisplayActionRefinementSelect);
			gui_state.AddState(tbbeCharSelModes.ItemSelect				,	DisplayItemSelect);
			gui_state.AddState(tbbeCharSelModes.TargetSelect);
			
			//configure screen size
			if (fixed_screen_area)
				GUIX.SetScreenSize(screen_width, screen_height);
		}
		
		void OnGUI()
		{
			if (!_ready)
				return;
			
			// we only want to see the player selection window when we are in player selection state
			// if we are not, then Deactivate the window
			if (!tbbBattleField.game_state.CompareState(tbbeBattleState.Player))
			{
				if (player_area.slideState.CompareState(eSlideState.Opened))
					player_area.Deactivate();
				
				// but, while we are closing, we still want to show the content or else it will
				// just vanish. So, while the window is closing, draw it but when closed, exit.
				// player_area.Deactivate (called above) set the state to Closing and the window
				// itself will set the state to Closed when appropriate
				if (!player_area.slideState.CompareState(eSlideState.Closing))
					return;
			}
			
			GUI.skin = battle_skin;
			
			if (fixed_screen_area)
				GUIX.FixScreenSize();
			
			DisplayPlayerSelect();
			
			DisplayTargetSelect();
			if (null != gui_state )
				gui_state.PerformAction();
		}
		
		void Update()
		{
			//in case the character select component has not been configured yet, quit
			if (null == tbbBattleField.game_state)
				return;
			
			//if the turn is now complete and the character select window is still open, close it
			if (tbbBattleField.game_state.CompareState(tbbeBattleState.Switching)
			    && player_area.slideState.CompareState(eSlideState.Opened))
				player_area.Deactivate();
			
			//if we are in the character select state but the window is still close, activate it
			if (tbbBattleField.game_state.CompareState(tbbeBattleState.Player) && player_area.slideState.CompareState(eSlideState.Closed))
				player_area.Activate();
			
			//update any animations as required...
			enemy_area.Update();
			player_area.Update();
		}
		
		void OnCharSelectStateChanged(tbbeCharSelModes state)
		{
			switch(state)
			{
			case tbbeCharSelModes.TargetSelect:
				enemy_area.Activate();
				break;
				
			default:
				if (enemy_area.slideState.CompareState(eSlideState.Opened) || enemy_area.slideState.CompareState(eSlideState.Opening) )
					enemy_area.Deactivate();
				break;
			}
		}
		
		/// <summary>
		/// This is triggered by tbbCharSelect after a character was selected.
		/// This function clears the field's current tinting then tints the selected character's tile to highlighted
		/// </summary>
		/// <param name="index">Index.</param>
		void OnCharacterSelected(int index)
		{
			tbbBattleField.active_faction.grid.ResetTileModes();
			tbbBattleField.active_faction.grid.TintCharacterTiles(tbbBattleField.active_faction.participants[SelectedChar],
			                                                      tbbETileMode.Highlighted);
		}
		
		void DisplayPlayerSelect()
		{
			player_area.FadeGUI();
			GUI.BeginGroup(player_area.Pos);
			int player_index = 0;
			foreach(tbbPlayerInfo player in tbbBattleField.active_faction.participants)
			{
				bool available = !(tbbBattleField.active_faction.CharacterInBattleOrderList(player) || tbbBattleField.active_faction.CharacterAwaitingAttackTurn(player) );
				
				GUI.enabled = available;
				if (SelectedChar == player_index && !available && char_select.char_sel_state.CompareState(tbbeCharSelModes.CharacterSelect))
				{
					char_select.SelectNextCharacter();
				}
				DisplayPlayerInfo(player_index++, player);
				GUI.enabled = true;
			}
			GUI.EndGroup();
			player_area.FadeGUI(false);
		}
		
		void DisplayActionSelect()
		{
			GUI.Box(player_action_areas[SelectedChar],"");
			GUI.BeginGroup(player_action_areas[SelectedChar]);
			float y = 0;
			for (tbbeActionType index = (tbbeActionType)0; index < tbbeActionType.Count; index++)
			{
				if (GUI.Button(new Rect(5f, 5f + ( y++ * FontSize) , player_action_areas[SelectedChar].width - 10f, FontSize), index.ToString()))
				{
					SelectedAction = (int)index;
					
					switch (index)
					{
					case tbbeActionType.Attack:
						char_select.SelectActionAttack();
						break;
						
					case tbbeActionType.Magic:
						char_select.SelectActionMagic();
						break;
					}
				}
			}
			
			GUI.EndGroup();
		}
		
		void DisplayActionRefinementSelect()
		{
			GUI.enabled = false;
			DisplayActionSelect();
			GUI.enabled = true;
			
			GUI.Box(player_action_submenu_areas[SelectedChar],"");
			GUI.BeginGroup(player_action_submenu_areas[SelectedChar]);
			switch((tbbeActionType)SelectedAction)
			{
			case tbbeActionType.Attack:
				for (int index = 0; index < LastEntry.character.attacks.Length; index++)
				{
					if (GUI.Button(new Rect(5f, 5f + ( index * FontSize) , player_action_submenu_areas[SelectedChar].width - 10f, FontSize), LastEntry.character.attacks[index].attack_name ))
					{
						SelectedAttack = index;
						tbbGUICursor.SetCursorPos(player_action_submenu_areas[SelectedChar]); 
						char_select.SelectActionAttackRefined();
					}
				}
				break;
				
			case tbbeActionType.Magic:
				for (int index = 0; index < LastEntry.character.spells.Length; index++)
				{
					int mp_r = LastEntry.character.spells[index].mp_required;
					int mp_h = LastEntry.character.MP;
					
					GUI.enabled = mp_h>= mp_r;
					if (GUI.Button(new Rect(5f, 5f + ( index * FontSize) , player_action_submenu_areas[SelectedChar].width - 10f, FontSize),
					               (show_mp ? ( "("+ mp_r+ ") " ) : "") + LastEntry.character.spells[index].attack_name ))
					{
						SelectedAttack = index;
						tbbGUICursor.SetCursorPos(player_action_submenu_areas[SelectedChar]); 
						char_select.SelectActionAttackRefined();
					}
					GUI.enabled = true;
				}
				break;
			}
			
			GUI.EndGroup();
		}
		
		void DisplayItemSelect()
		{
			
		}
		
		void DisplayTargetSelect()
		{
			if (enemy_area.slideState.CompareState(eSlideState.Closed))
				return;
			
			int index  = 0;
			GUI.BeginGroup(enemy_area.Pos);
			foreach(tbbPlayerInfo enemy in tbbBattleField.opponent.participants)
			{
				DisplayPlayerDetails(enemy, enemy_info_areas[index++] );
			}
			GUI.EndGroup();
		}
		
		void DisplayPlayerInfo(int index, tbbPlayerInfo player)
		{
			if (index >= player_info_areas.Length)
				return;
			
			DisplayPlayerDetails(player, player_info_areas[index]);
		}
		
		void DisplayHealthBar(Rect pos, Rect bar_offset, Color tint, float value, float max, string stat, Rect stat_pos)
		{
			float percentage = bar_offset.width * (value / max);
			
			GUI.DrawTexture(pos, health_bar_bg);
			GUI.color = tint;
			GUI.BeginGroup(new Rect(pos.x + bar_offset.x, pos.y + bar_offset.y, percentage, bar_offset.height));
			GUI.DrawTexture(new Rect(0,0,bar_offset.width,bar_offset.height), health_bar_img);
			GUI.EndGroup();
			GUI.color = Color.white;
			GUI.Label (stat_pos, stat);
			pos.x += 10f;
			GUI.Label (pos, value +"/"+max);
		}
		
		void DisplayPlayerDetails(tbbPlayerInfo character, Rect area)
		{
			GUI.Box (area, "");
			GUI.BeginGroup(area);
			
			if (null != character.avatar)
				GUI.DrawTexture(avatar_area, character.avatar);
			
			DisplayHealthBar(hp_bar_area, healthbar_offset, HP_color, character.HP, character.MaxHP, "HP", new Rect(hp_bar_area.x - 30, hp_bar_area.y, 50, FontSize));
			DisplayHealthBar(mp_bar_area, healthbar_offset, MP_color, character.MP, character.MaxMP, "MP", new Rect(mp_bar_area.x - 30, mp_bar_area.y, 50, FontSize));
			
			GUI.EndGroup ();
		}
	}
}
