using UnityEngine;
using System.Collections;

namespace MBS {
	[RequireComponent(typeof(tbbCharSelectGUI))]
	/// <summary>
	///  This class is the GUI component of the cursor class. The cursor class handles position while this one displays it on screen
	/// </summary>
	public class tbbGUICursor : MonoBehaviour {
		
		static tbbGUICursor _instance;
		static public tbbGUICursor Instance
		{
			get 
			{
				if (null == _instance)
				{
					tbbGUICursor[] objs = GameObject.FindObjectsOfType<tbbGUICursor>();
					if (null == objs)
						return null;
					_instance = objs[0];
				}
				return _instance;
			}
		}
		
		tbbCharSelectGUI
			battle_gui;
		
		/// <summary>
		/// When false the cursor is not drawn to the display
		/// </summary>
		[System.NonSerialized] public bool 
			Display;
		
		/// <summary>
		/// Returns the cursor position factoring in the specified area for displaying
		/// all party member infos and the currently selected party member
		/// </summary>
		public Rect CurrentPlayerPosition { get { 		
				if (null == battle_gui)
					return new Rect(0,0,0,0);
				
				Rect temp = battle_gui.player_info_areas[tbbCharSelect.Instance.selected_char];
				temp.x += battle_gui.player_area.targetPos.x;
				temp.y += battle_gui.player_area.targetPos.y;
				return temp;
			} }
		
		/// <summary>
		/// The icon to use for the on screen hand-cursor
		/// </summary>
		public Texture2D
			cursor;
		
		/// <summary>
		/// The size and offset of the cursor from the top left corner of the character select area
		/// </summary>
		public Rect
			cursor_offset_char_select = new Rect(45,20,32,32);
		
		
		/// <summary>
		/// How fast the cursor bounce animation plays
		/// </summary>
		public float	cursor_speed = 40f;
		/// <summary>
		/// How far to animate the bouncing cursor
		/// </summary>
		public float	cursor_spring_distance = 20f;
		
		Rect
			cursor_target_pos;
		
		float 
			cursor_spring_dir = -1f,
			cursor_spring_offset = 0f;
		
		bool 
			_ready = false;
		
		mbsStateMachineLeech<tbbeCharSelModes>
			state;
		
		/// <summary>
		/// Factoring in the min and max position the cursor can have while bouncing,
		/// where should it actually be draw right now ?
		/// </summary>
		/// <value>The cursor_actual_pos.</value>
		protected Rect cursor_actual_pos
		{
			get 
			{
				Rect result = cursor_target_pos;
				result.x += cursor_spring_offset;
				return result; 
			} 
		}
		
		/// <summary>
		/// Tell the class it can now start referencing the tbbBattleField component's state variable
		/// </summary>
		void OnBattleSystemInitialized()
		{
			_ready = true;
		}
		
		void Start () {
			battle_gui = GetComponent<tbbCharSelectGUI>();		
			_instance = this;
		}
		
		/// <summary>
		/// Factor in the min and max positions the cursor could be and either move towards
		/// one of the limits or change direction if that limit is overshot
		/// </summary>
		void Update () {
			cursor_spring_offset += cursor_spring_dir * cursor_speed * Time.deltaTime;
			if (cursor_spring_offset < -cursor_spring_distance)
			{
				cursor_spring_offset = -cursor_spring_distance;
				cursor_spring_dir = 1;
			} else
				if (cursor_spring_offset > 0)
			{
				cursor_spring_offset = 0;
				cursor_spring_dir = -1;
			}
		}
		
		/// <summary>
		/// Handle the cursor position when selecting a character...
		/// Each state has it's own cursor position handling function but the character select state is
		/// the base state and is thus not handled by anything so we take care of that ourselves in here
		/// </summary>
		/// <param name="index">Selected_character.</param>
		void OnCharacterSelected(int selected_character)
		{
			switch(state.CurrentState)
			{
			case tbbeCharSelModes.CharacterSelect:
				SetCursorPos(CurrentPlayerPosition);
				break;
			}
		}
		
		/// <summary>
		/// After the state changed away from the player's turn to select the characters, this script is
		/// effectively disabled. When the player's turn comes up again, this function enables the script
		/// so the cursor immediately skips to the first character instead of only showing after the player
		/// has made his first selection...
		/// </summary>
		void OnBattleFieldStateChangedToPlayer()
		{
			Display = true;
		}
		
		/// <summary>
		//	Once the tbbCharSelect component has initialized itself, setup a leech to start
		//	watching it's state so OnCharSelectStateChanged can function properly
		/// </summary>
		void OnCharacterSelectInitialized()
		{
			state = new mbsStateMachineLeech<tbbeCharSelModes>(tbbCharSelect.Instance.char_sel_state);
		}
		
		/// <summary>
		/// This function sets the cursor to the default location within a new selection area when the
		/// state changes from character select to action select to action refinement select etc...
		/// </summary>
		/// <param name="state">State.</param>
		public void OnCharSelectStateChanged(tbbeCharSelModes state)
		{
			switch(state)
			{
			case tbbeCharSelModes.CharacterSelect:
				SetCursorPos ( CurrentPlayerPosition);
				
				break;
				
			case tbbeCharSelModes.ActionSelect:
				SetCursorPos ( battle_gui.player_action_areas[tbbCharSelect.Instance.selected_char]);
				break;
				
			case tbbeCharSelModes.TargetSelect:
				SetCursorPos ( battle_gui.enemy_area.targetPos);
				break;
			}
		}
		
		/// <summary>
		/// This function is used to actually set the position of the cursor on screen.
		/// This function then factors in the current "bounce" position of the cursor from here
		/// and sets the "real" position to draw the cursor at
		/// </summary>
		/// <param name="target">Target_pos.</param>
		static public void SetCursorPos(Rect target_pos)
		{
			Instance.cursor_target_pos = Instance.cursor_offset_char_select;
			Instance.cursor_target_pos.x += target_pos.x;
			Instance.cursor_target_pos.y += target_pos.y;
		}
		
		void OnGUI()
		{
			if (!_ready || !tbbBattleField.game_state.CompareState(tbbeBattleState.Player) || !Display)
				return;
			
			if (battle_gui.fixed_screen_area)
				GUIX.FixScreenSize();
			GUI.depth = 0;
			GUI.DrawTexture(cursor_actual_pos, cursor); 
		}
	}
}
