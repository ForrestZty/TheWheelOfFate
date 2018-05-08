using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// This class handles some of the common functions associated with character selection.
	/// It is made to be distinct from the GUIs so GUIS can be replaced and still be placed
	/// on top of this class.
	/// </summary>
	[RequireComponent (typeof(tbbBattleField)) ]
	public class tbbCharSelect : MonoBehaviour {
		
		static tbbCharSelect _instance;
		static public tbbCharSelect Instance
		{ 
			get 
			{
				if (null == _instance)
				{
					//see if there is one in the scene but if not, don't create one.
					//if this causes an error it is meant to tell the developer the prefab is not properly built
					_instance = GameObject.FindObjectOfType<tbbCharSelect>();
				}
				return _instance;
			} 
		}
		
		
		/// <summary>
		/// Index into the selected character from the Party.participants
		/// </summary>
		[System.NonSerialized] public int	selected_char;

		/// <summary>
		/// Index into the selected character from Opponents.participants
		/// </summary>
		[System.NonSerialized] public int	selected_target;

		/// <summary>
		/// Index into what type of attack to perform
		/// </summary>
		[System.NonSerialized] public int	selected_attack;
		
		/// <summary>
		/// A property that returns the number of characters left in Opponent.participants
		/// </summary>
		/// <value>The enemy count.</value>
		public int EnemyCount { get { return tbbBattleField.opponent.participants.Count; }  }
		
		
		[System.NonSerialized]
		public List<tbbPlayerInfo>
			/// <summary>
			/// An array listing the characters that have been selected and configred (or still being configured) to go into battle
			/// </summary>
			battle_order;
		
		/// <summary>
		/// Indicates the current state of the character select system.
		/// I.e. are you selecting your character, a move, a target, etc.
		/// </summary>
		public mbsStateMachine<tbbeCharSelModes>
			char_sel_state;
		
		void OnBattleSystemInitialized()
		{
			//setup empty battle order lists
			battle_order = new List<tbbPlayerInfo>();
			selected_char = 0;
			
			//configure state machines
			char_sel_state = new mbsStateMachine<tbbeCharSelModes>();
			for(tbbeCharSelModes index = (tbbeCharSelModes)0; index < tbbeCharSelModes.Count; index++)
				char_sel_state.AddState(index);
			
			tbbBattleField.Instance.doOnCharSelectInitialized();
		}
		
		void OnCharacterSelectInitialized()
		{
			ChangeToState(tbbeCharSelModes.CharacterSelect);
		}
		
		void OnBattleFieldStateChangedToIntro2()
		{
			selected_attack = 0;
			selected_target = 0;
			SelectCharacter(0, true, 0);
		}
		
		/// <summary>
		/// Use this inside your character Select GUI component to select the next available character
		/// from the party's participants. Will wrap to the first character if it goes over
		/// </summary>
		public void SelectNextCharacter()
		{
			SelectCharacter(++selected_char, true, 0);
		}
		
		/// <summary>
		/// Use this inside your character Select GUI component to select the previous available character
		/// from the party's participants. Will wrap to the last character if it goes under 0
		/// </summary>
		public void SelectPreviousCharacter()
		{
			SelectCharacter(--selected_char, false, 0);
		}
		
		/// <summary>
		/// Specify a character to use for this turn
		/// </summary>
		/// <param name="index">Indexed by order in the participants list.</param>
		/// <param name="next_on_fail">If set to <c>true</c> and the current character is not available, will select the next character, otherwise it will select the previous one</param>
		/// <param name="counter">Used internally. If you call this function, always set this to 0</param>
		public void SelectCharacter(int index, bool next_on_fail, int counter)
		{
			int count = tbbBattleField.active_faction.participants.Count;
			if (index < 0)
				index = count - 1;
			else
				if (index > count - 1)
					index = 0;
			
			if (tbbBattleField.active_faction.SpecifiedBattleOrderEntry( tbbBattleField.active_faction.participants[index] ) != null)
			{
				//if this function has been called recursively for more times
				//than there are characters in the field, then all characters are
				//unavailable so don't try to select a valid one any more...
				if (counter++ >= count)
					return;
				
				if (next_on_fail)
					SelectCharacter(++index, next_on_fail, counter);
				else
					SelectCharacter(--index, next_on_fail, counter);
			} else
			{
				selected_char = index;
				tbbBattleField.Instance.doOnCharacterSelected(selected_char);
			}
		}
		
		public void SelectCharacterAndContinue(tbbPlayerInfo character, tbbeActionType type)
		{
			tbbBattleField.active_faction.AddCharacterToBattleOrder(character, null, type);
			if (tbbBattleField.Instance.allow_repositioning)
				ChangeToState(tbbeCharSelModes.NewLocationSelect);
			else
				ChangeToState(tbbeCharSelModes.ActionSelect);
		}
		
		
		/// <summary>
		/// A Convenience function to make it simpler to go through states sequentially
		/// </summary>
		public void ChangeToNextState()
		{
			tbbeCharSelModes  next = char_sel_state.CurrentState+1;
			if (next == tbbeCharSelModes.Count)
				next = (tbbeCharSelModes)0;
			ChangeToState(next);
		}
		
		/// <summary>
		/// A Convenience function to make it simpler to go through states sequentially
		/// </summary>
		public void ChangeToPreviousState()
		{
			tbbeCharSelModes  next = char_sel_state.CurrentState;
			if (next == (tbbeCharSelModes)0 )
				next = tbbeCharSelModes.Count - 1;
			else 
				next--;
			ChangeToState(next);
		}
		
		/// <summary>
		/// Sets the state of the character select component. I.e. select characater, select action, select target etc.
		/// </summary>
		/// <param name="state">The new state. See <c>tbbeCharSelModes</c></param>
		public void ChangeToState(tbbeCharSelModes state)
		{
			tbbeCharSelModes old_state = char_sel_state.CurrentState;
			
			//first change the state of the character select system
			char_sel_state.SetState(state);
			
			//do a little bit of error checking
			//			if (null != tbbBattleField.active_faction)
			if (tbbBattleField.active_faction.participants.Count <= selected_char)
			{
				StatusMessage.Message = "There are only " + 
					tbbBattleField.active_faction.participants.Count +
						" characters and you are selecting number " + selected_char;
				return;
			}
			
			//do some processing depending on what the new state is...
			switch(state)
			{
			case tbbeCharSelModes.CharacterSelect:
				tbbBattleField.Instance.grid.ResetTileModes();
				if (tbbBattleField.active_faction.LastBattleOrderEntry && !tbbBattleField.active_faction.LastBattleOrderEntry.Configured)
					tbbBattleField.active_faction.grid.TintCharacterTiles( tbbBattleField.active_faction.participants[selected_char],
					                                                      tbbETileMode.Highlighted);
				break;
				
			case tbbeCharSelModes.NewLocationSelect:
				//if going from character select to movement select, store the current position and rotation, etc
				//but don't do that when coming from action select back to movement select
				if (old_state < state)
				{
					if (null != tbbMovement.ActiveCharacter)
						tbbMovement.ActiveCharacter.StoreOrigin();
				}
				break;
				
			case tbbeCharSelModes.ActionSelect:
				tbbBattleField.active_faction.LastBattleOrderEntry.character.selected_action = selected_attack = 0;
				break;
				
			case tbbeCharSelModes.ActionRefinementSelect:
				selected_attack = 0;
				break;
				
			case tbbeCharSelModes.TargetSelect:
				selected_target = 0;
				if (tbbBattleField.opponent.participants.Count > 0)
					tbbBattleField.opponent.grid.TintCharacterTiles(tbbBattleField.opponent.participants[selected_target],
					                                                tbbETileMode.ValidSelection);
				
				break;
			}
			
			//...and then change the state of the battlefield itself
			tbbBattleField.Instance.doOnCharSelectStateChange(state);
		}
		
		/// <summary>
		/// specify what type of action the player has chosen
		/// </summary>
		/// <param name="action">Int value representing <c>tbbeActionType<c/>.Melee, Magic or other (more to come in future updates)</param>
		/// <param name="index">Which, of the available attacks for the selected action type, was selected</param>
		virtual public void SelectAction(int action, int index)
		{
			SelectAction((tbbeActionType)action, index);
		}
		
		/// <summary>
		/// specify what type of action the player has chosen
		/// </summary>
		/// <param name="action">Enum of type <c>tbbeActionType</c>Melee, Magic or other (more to come in future updates)</param>
		/// <param name="index">Which, of the available attacks for the selected action type, was selected</param>
		virtual public void SelectAction( tbbeActionType action, int index)
		{
			tbbBattleField.active_faction.LastBattleOrderEntry.action = action;
			switch(action)
			{
			case tbbeActionType.Attack:
				tbbBattleField.active_faction.LastBattleOrderEntry.AssignAttack( tbbBattleField.active_faction.LastBattleOrderEntry.character.attacks[index] );
				ChangeToState(tbbeCharSelModes.TargetSelect);
				break;
				
			case tbbeActionType.Magic:
				tbbBattleField.active_faction.LastBattleOrderEntry.AssignSpell( tbbBattleField.active_faction.LastBattleOrderEntry.character.spells[index] );
				ChangeToState(tbbeCharSelModes.TargetSelect);
				break;
				
				/*			case tbbeActionType.Item:
				ChangeToState(tbbeCharSelModes.ItemSelect);
				break;
				
			case tbbeActionType.Flee:
//				ChangeToState(tbbeCharSelModes.);
				break;
				
			case tbbeActionType.Protect:
//				ChangeToState(tbbeCharSelModes.);
				break;
*/			}
			
			tbbBattleField.Instance.doOnActionSelected(action);
		}
		
		/// <summary>
		/// This function calls SelectAction and specifies you selected a Melee type attack and which attack was selected
		/// </summary>
		public void SelectActionAttackRefined()
		{
			SelectAction((tbbeActionType)tbbBattleField.active_faction.LastBattleOrderEntry.character.selected_action, selected_attack);
		}
		
		/// <summary>
		/// This function checks to see if the action type you selected has multiple entries and if not, it calls SelectAction
		/// and specifies you selected a Melee type attack and which attack was selected. If it does have multiple entries, it
		/// changes to the refinement state for you to first clarify which entry you want to use.
		/// </summary>
		public void SelectActionAttack()
		{
			if (tbbBattleField.active_faction.LastBattleOrderEntry.character.attacks.Length > 1)
				ChangeToState(tbbeCharSelModes.ActionRefinementSelect);
			else
				SelectAction(tbbBattleField.active_faction.LastBattleOrderEntry.character.selected_action, selected_attack);
		}
		
		/// <summary>
		/// This function checks to see if the action type you selected has multiple entries and if not, it calls SelectAction
		/// and specifies you selected a Magic type attack and which spell was selected. If it does have multiple entries, it
		/// changes to the refinement state for you to first clarify which entry you want to use.
		/// </summary>
		public void SelectActionMagic()
		{
			//uncomment these lines if you want the spell auto selected if there is only one
			//			if (tbbBattleField.active_faction.LastBattleOrderEntry.character.spells.Length > 1)
			ChangeToState(tbbeCharSelModes.ActionRefinementSelect);
			//			else
			//				SelectAction(tbbBattleField.active_faction.LastBattleOrderEntry.character.selected_action, selected_attack);
		}
		
		/// <summary>
		///	Determine the next value based on keyboard input and selected action type
		/// </summary>
		/// <param name="next">If set to <c>true</c> attempt to select the next value, else select the previous value.
		/// In both cases, wrap the results to the available limits as defined by the selected action</param>
		public void UpdateActionRefinementSelectWrapped(){UpdateActionRefinementSelectWrapped(true);}
		public void UpdateActionRefinementSelectWrapped(bool next)
		{
			int max = 0;
			switch ((tbbeActionType)tbbBattleField.active_faction.LastBattleOrderEntry.character.selected_action)
			{
			case tbbeActionType.Attack	: max = tbbBattleField.active_faction.LastBattleOrderEntry.character.attacks.Length; break;
			case tbbeActionType.Magic	: max = tbbBattleField.active_faction.LastBattleOrderEntry.character.spells.Length; break;
				//more to follow...
			}
			if (next)
				selected_attack = selected_attack < max - 1 ? selected_attack + 1 : 0;
			else
				selected_attack = selected_attack > 0 ? selected_attack - 1 : max - 1;
		}
		
	}
}
