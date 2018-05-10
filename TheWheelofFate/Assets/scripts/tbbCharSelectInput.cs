using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	
	/// <summary>
	/// This is the Input component for the included tbbCharacterSelectGUI.
	/// Most likely if you replace that component you will need a new version of this to go with it.
	/// This also updates the tbbCharSelect class as appropriate
	/// This class just calls functions inside tbbcharSelect and updates the cursor position and contains
	/// no public functions
	/// </summary>
	[RequireComponent ( typeof ( tbbCharSelect ) )]
	public class tbbCharSelectInput : MonoBehaviour {
		
		tbbCharSelect char_select { get { return tbbCharSelect.Instance; } }
		
		tbbCharSelectGUI
			char_select_gui;
		
		tbbGUICursor
			cursor;
		
		[System.NonSerialized]
		mbsStateMachineLeech<tbbeCharSelModes>
			keyboard_input;
		
		// These are all convenience functions to make the code a little shorter and easier to read
		int SelectedChar			{ get { return char_select.selected_char;	} set { char_select.selected_char	= value;} }
		int SelectedAction			{ get { return LastEntry.character.selected_action; } set { LastEntry.character.selected_action = value;} }
		int SelectedAttack			{ get { return char_select.selected_attack; } set { char_select.selected_attack = value;} }
		int SelectedTarget			{ get { return char_select.selected_target; } set { char_select.selected_target = value;} }
		tbbBattleOrder LastEntry	{ get { return tbbBattleField.active_faction.LastBattleOrderEntry;}}

		
		void OnCharacterSelectInitialized()
		{
			keyboard_input = new mbsStateMachineLeech<tbbeCharSelModes>(char_select.char_sel_state);
			keyboard_input.AddState(tbbeCharSelModes.CharacterSelect			,	TestKeyboardInputCharSelect);
			keyboard_input.AddState(tbbeCharSelModes.NewLocationSelect			,	TestKeyboardInputNewLocation);
			keyboard_input.AddState(tbbeCharSelModes.ActionSelect				,	TestKeyboardInputAction);
			keyboard_input.AddState(tbbeCharSelModes.ActionRefinementSelect		,	TestKeyboardInputActionRefinement);
			keyboard_input.AddState(tbbeCharSelModes.SpellSelect				,	TestKeyboardInputAction);
			keyboard_input.AddState(tbbeCharSelModes.ItemSelect					,	TestKeyboardInputAction);
			keyboard_input.AddState(tbbeCharSelModes.TargetSelect				,	TestKeyboardInputTarget);
		}
		
		// Called from tbbBattleField. If you make your own CharacterSelectGUI component, make sure to include this function
		void SetupReferences()
		{
			char_select_gui = GetComponent<tbbCharSelectGUI>();
			cursor			= GetComponent<tbbGUICursor>();
		}
		
		void OnBattleSystemInitialized ()
		{
		}
		
		void Update () {
			if (	null != keyboard_input 
			    && 	tbbBattleField.game_state.CompareState(tbbeBattleState.Player) 
			    &&	tbbBattleField.active_faction.Intelligence == tbbeControlMethod.Human)
				keyboard_input.PerformAction();
		}
		
		void ValidateCursorVisibility()
		{
			if (SelectedChar >= tbbBattleField.active_faction.participants.Count)
			{
				cursor.Display = false;
				return;
			}
			
			tbbBattleOrder entry = tbbBattleField.active_faction.SpecifiedBattleOrderEntry(tbbBattleField.active_faction.participants[SelectedChar]);
			if (null == entry)
				cursor.Display = true;
			else
				cursor.Display = entry.character.attack_state == tbbeAttackState.Config;
		}
		
		public void TestKeyboardInputCharSelect()
		{
			if (tbbButtonMap.left_button_up || tbbButtonMap.up_button_up)
			{
				if (SelectedChar < tbbBattleField.active_faction.participants.Count)
					tbbBattleField.active_faction.grid.TintCharacterTiles(tbbBattleField.active_faction.participants[SelectedChar], tbbETileMode.Normal);
				char_select.SelectPreviousCharacter();
				
				ValidateCursorVisibility();
				
				if (SelectedChar < tbbBattleField.active_faction.participants.Count && null == tbbBattleField.active_faction.participants[SelectedChar].target)
					tbbBattleField.active_faction.grid.TintCharacterTiles(tbbBattleField.active_faction.participants[SelectedChar], tbbETileMode.Highlighted);
			}
			
			if (tbbButtonMap.right_button_up || tbbButtonMap.down_button_up )
			{
				if (SelectedChar < tbbBattleField.active_faction.participants.Count)
					tbbBattleField.active_faction.grid.TintCharacterTiles(tbbBattleField.active_faction.participants[SelectedChar], tbbETileMode.Normal);
				char_select.SelectNextCharacter();
				
				ValidateCursorVisibility();
				
				if (SelectedChar < tbbBattleField.active_faction.participants.Count && null == tbbBattleField.active_faction.participants[SelectedChar].target)
					tbbBattleField.active_faction.grid.TintCharacterTiles(tbbBattleField.active_faction.participants[SelectedChar], tbbETileMode.Highlighted);
				
			}
			
			if (tbbButtonMap.action_button_up)
			{
				if (SelectedChar < 0 || SelectedChar >= tbbBattleField.active_faction.participants.Count)
					return;

				char_select.SelectCharacterAndContinue(tbbBattleField.active_faction.participants[SelectedChar], tbbeActionType.Attack);
			}
			
			if (tbbButtonMap.cancel_button_up)
			{
				if (null != tbbBattleField.active_faction.battle_order
				    && tbbBattleField.active_faction.battle_order.Count > 0 
				    && tbbBattleField.Instance.battle_type == tbbeBattleType.PerFaction )
				{
					tbbBattleField.active_faction.grid.ResetTileModes();
					tbbBattleField.active_faction.LastBattleOrderEntry.MarkAsConfigured(false);
					
					if ((tbbBattleField.active_faction.CountBattleOrderEntries > 1
					     &&   tbbBattleField.active_faction.battle_order[ tbbBattleField.active_faction.CountBattleOrderEntries - 2 ].character.attack_state < tbbeAttackState.Done)
					    ||   tbbBattleField.active_faction.CountBattleOrderEntries == 1)
					{
						SelectedChar = tbbBattleField.active_faction.SpecifiedBattleOrderEntryi( tbbBattleField.active_faction.LastBattleOrderEntry.character );
						tbbBattleField.active_faction.grid.TintCharacterTiles( tbbBattleField.active_faction.LastBattleOrderEntry.character, tbbETileMode.Highlighted);
						char_select.ChangeToState(tbbeCharSelModes.TargetSelect);
					} else
					{
						char_select.ChangeToState(tbbeCharSelModes.CharacterSelect);
					}
				}
			}
		}

		void TestKeyboardInputNewLocation()
		{
			if (tbbButtonMap.action_button_up)
			{
				tbbMovement.ActiveCharacter.LockDownNewPosition();
			}

			if (tbbButtonMap.left_button_up) tbbMovement.ActiveCharacter.MoveSelectionLeft();
			if (tbbButtonMap.right_button_up) tbbMovement.ActiveCharacter.MoveSelectionRight();
			if (tbbButtonMap.up_button_up) tbbMovement.ActiveCharacter.MoveSelectionUp();
			if (tbbButtonMap.down_button_up) tbbMovement.ActiveCharacter.MoveSelectionDown();

			if (tbbButtonMap.cancel_button_up)
			{
				tbbMovement.ActiveCharacter.RevertToPreviousState();
				tbbBattleField.active_faction.LastBattleOrderEntry.MarkAsConfigured(false);
				tbbBattleField.active_faction.RemoveLastCharacterFromBattleOrder();
				char_select.SelectCharacter(SelectedChar,true, 0);
				char_select.ChangeToPreviousState();
			}
		}
		
		void TestKeyboardInputAction()
		{
			if (tbbButtonMap.left_button_up || tbbButtonMap.up_button_up)
			{
				SelectedAction = SelectedAction > 0 ? SelectedAction - 1 : (int)tbbeActionType.Count - 1;
				Rect new_pos = char_select_gui.player_action_areas[SelectedChar];
				new_pos.y += (SelectedAction * char_select_gui.FontSize);
				tbbGUICursor.SetCursorPos(new_pos);
			}
			
			if (tbbButtonMap.right_button_up || tbbButtonMap.down_button_up)
			{
				SelectedAction = SelectedAction < (int)tbbeActionType.Count - 1 ? SelectedAction + 1 : 0;
				Rect new_pos = char_select_gui.player_action_areas[SelectedChar];
				new_pos.y += (SelectedAction * char_select_gui.FontSize);
				tbbGUICursor.SetCursorPos(new_pos);
			}
			
			if (tbbButtonMap.action_button_up )
			{
				tbbGUICursor.SetCursorPos(char_select_gui.player_action_submenu_areas[SelectedChar]); 
				switch((tbbeActionType)SelectedAction)
				{
				case tbbeActionType.Attack:
					char_select.SelectActionAttack();
					break;
					
				case tbbeActionType.Magic:
					char_select.SelectActionMagic();
					break;
				}
			}
			
			if (tbbButtonMap.cancel_button_up)
			{
				if (tbbBattleField.Instance.allow_repositioning)
				{
					char_select.ChangeToState(tbbeCharSelModes.NewLocationSelect);
				} 
				else
				{
					tbbBattleField.active_faction.LastBattleOrderEntry.MarkAsConfigured(false);
					tbbBattleField.active_faction.RemoveLastCharacterFromBattleOrder();
					char_select.SelectCharacter(SelectedChar,true, 0);
					char_select.ChangeToState(tbbeCharSelModes.CharacterSelect);
				}
			}
		}
		
		void TestKeyboardInputActionRefinement()
		{
			if (tbbButtonMap.left_button_up || tbbButtonMap.up_button_up)
			{
				char_select.UpdateActionRefinementSelectWrapped(false);
				Rect new_pos = char_select_gui.player_action_submenu_areas[SelectedChar];
				new_pos.y += (SelectedAttack * char_select_gui.FontSize);
				tbbGUICursor.SetCursorPos(new_pos);
			}
			
			if (tbbButtonMap.right_button_up || tbbButtonMap.down_button_up)
			{
				char_select.UpdateActionRefinementSelectWrapped();
				Rect new_pos = char_select_gui.player_action_submenu_areas[SelectedChar];
				new_pos.y += (SelectedAttack * char_select_gui.FontSize);
				tbbGUICursor.SetCursorPos(new_pos);
			}
			
			if (tbbButtonMap.action_button_up)
			{
				tbbGUICursor.SetCursorPos(char_select_gui.player_action_submenu_areas[SelectedChar]); 
				char_select.SelectActionAttackRefined();
			}
			
			if (tbbButtonMap.cancel_button_up)
			{
				char_select.ChangeToState(tbbeCharSelModes.ActionSelect);
			}
		}
		
		void TestKeyboardInputTarget()
		{
			if (tbbButtonMap.left_button_up || tbbButtonMap.up_button_up)
			{
				tbbBattleField.opponent.grid.TintCharacterTiles(tbbBattleField.opponent.participants[SelectedTarget], tbbETileMode.Normal);
				SelectedTarget = SelectedTarget > 0 ? SelectedTarget - 1 : char_select.EnemyCount - 1;
				tbbBattleField.opponent.grid.TintCharacterTiles(tbbBattleField.opponent.participants[SelectedTarget], tbbETileMode.ValidSelection);
				
				Rect new_pos = char_select_gui.enemy_area.targetPos;
				new_pos.y += char_select_gui.enemy_info_areas[SelectedTarget].y;
				tbbGUICursor.SetCursorPos(new_pos); 
			}
			
			if (tbbButtonMap.right_button_up || tbbButtonMap.down_button_up)
			{
				tbbBattleField.opponent.grid.TintCharacterTiles(tbbBattleField.opponent.participants[SelectedTarget], tbbETileMode.Normal);
				SelectedTarget = SelectedTarget < char_select.EnemyCount - 1 ? SelectedTarget + 1 : 0;
				tbbBattleField.opponent.grid.TintCharacterTiles(tbbBattleField.opponent.participants[SelectedTarget], tbbETileMode.ValidSelection);
				
				Rect new_pos = char_select_gui.enemy_area.targetPos;
				new_pos.y += char_select_gui.enemy_info_areas[SelectedTarget].y;
				tbbGUICursor.SetCursorPos(new_pos); 
			}
			
			if (tbbButtonMap.action_button_up)
			{
				tbbBattleField.active_faction.LastBattleOrderEntry.SelectTarget(tbbBattleField.opponent.participants[SelectedTarget]);
				char_select.ChangeToState(tbbeCharSelModes.CharacterSelect);
			}
			
			if (tbbButtonMap.cancel_button_up)
			{
				tbbBattleField.opponent.grid.ResetTileModes();
				if (tbbBattleField.active_faction.LastBattleOrderEntry.character.attacks.Length > 1)
				{
					tbbGUICursor.SetCursorPos(char_select_gui.player_action_submenu_areas[SelectedChar]);
					char_select.ChangeToState(tbbeCharSelModes.ActionRefinementSelect);
				} else
				{
					char_select.ChangeToState(tbbeCharSelModes.ActionSelect);
				}
			}
		}
		
	}
}
