using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	
	/// <summary>
	/// This is one possible battle system this kit can handle.
	/// More game modes will be released with sequential numbers or subclass these
	/// BattleSystem1 is the most basic battel system. It handles the spawning
	/// of enemies and party members and basic combat giving you a complete
	/// start, fight and win/loose model to build custom battle systems on top of
	/// </summary>
	public class tbbBattleSystem1 : MonoBehaviour {
		
		tbbBattleField battle_field { get { return tbbBattleField.Instance; } } 
		
		mbsStateMachineLeech<tbbeBattleState>
			state;
		
		
		/// <summary>
		/// An image that tells the player a fight is to begin. By default it is text saying "Fight"
		/// </summary>
		public Texture2D	fight_image;
		/// <summary>
		/// An imge that appears between the two faction banners at the start of the battle.
		/// For instance "Player VS Enemy" would appear as 3 images. This specifies the middle one
		/// </summary>
		public Texture2D	vs_image;
		/// <summary>
		/// An image to display on screen when the battle was been won
		/// </summary>
		public Texture2D	victory_image;
		/// <summary>
		/// An image to display on screen if the battle was lost
		/// </summary>
		public Texture2D	defeat_image;
		
		
		/// <summary>
		/// Configuration of the opening and mid game banner images.
		/// Slide speed and direction as well as position.
		/// </summary>
		public tbbIntroText	opening_top;
		/// <summary>
		/// Configuration of the opening and mid game banner images.
		/// Slide speed and direction as well as position.
		/// </summary>
		public tbbIntroText	opening_bottom;
		/// <summary>
		/// Configuration of the opening and mid game banner images.
		/// Slide speed and direction as well as position.
		/// </summary>
		public tbbIntroText	opening_middle;
		
		void Start()
		{
			state = new mbsStateMachineLeech<tbbeBattleState>(tbbBattleField.game_state);
			for (tbbeBattleState s = (tbbeBattleState)0; s < tbbeBattleState.Count; s++)
				state.AddState(s);
		}
		
		/// <summary>
		/// This function is just a shortcut to changing the state of the battlefield.
		/// See tbbBattleField's ChangeToBattleState function for more details 
		/// </summary>
		/// <param name="state">State.</param>
		public void ChangeToBattleState(tbbeBattleState state)
		{
			tbbBattleField.ChangeToBattleState(state);
		}
		
		void OnIntro1Done()
		{
			ChangeToBattleState(tbbeBattleState.Switching);
		}
		
		void OnIntro2Done()
		{
			if (tbbBattleField.active_faction.Intelligence == tbbeControlMethod.Human)
				ChangeToBattleState(tbbeBattleState.Player);
			else 
				ChangeToBattleState(tbbeBattleState.AI);
		}
		
		void OnVictoryDone()
		{
			ChangeToBattleState(tbbeBattleState.ResultsScreen);
		}
		
		void OnDefeatDone()
		{
			ChangeToBattleState(tbbeBattleState.Retry);
		}
		
		void OnBattleFieldStateChangedToIntro1()
		{
			opening_top.onDone = OnIntro1Done;
			
			opening_top.msg_icon = tbbBattleField.Instance.top_faction_banner;
			opening_bottom.msg_icon = tbbBattleField.Instance.bottom_faction_banner;
			opening_middle.msg_icon = vs_image;
			
			opening_top.StartIntro();
			opening_bottom.StartIntro();
			opening_middle.StartIntro();
		}
		
		void OnBattleFieldStateChangedToIntro2()
		{
			opening_top.onDone = OnIntro2Done;
			
			opening_top.msg_icon = tbbBattleField.active_faction.faction_banner;
			opening_bottom.msg_icon = fight_image;
			
			opening_top.StartIntro();
			opening_bottom.StartIntro();
		}
		
		void OnBattleFieldStateChangedToSwitching()
		{
			if (null != tbbBattleField.active_faction)
				tbbBattleField.active_faction.ClearBattleOrder();
			
			if (null == tbbBattleField.active_faction)
				tbbBattleField.active_faction = battle_field.StartingFaction == tbbeBattlefieldSide.Lower ? tbbBattleField.Instance.BottomFaction : tbbBattleField.Instance.TopFaction;
			else
				tbbBattleField.active_faction = tbbBattleField.opponent;
			
			foreach(tbbPlayerInfo participant in tbbBattleField.active_faction.participants)
				if (participant.attack_state != tbbeAttackState.Dead)
					participant.attack_state = tbbeAttackState.Config;
			
			ChangeToBattleState(tbbeBattleState.Intro2);
		}
		
		void OnBattleFieldStateChangedToPlayer()
		{
			
		}
		
		void OnBattleFieldStateChangedToAI	  ()
		{
			tbbFaction AI_faction = tbbBattleField.active_faction;
			foreach(tbbPlayerInfo participant in AI_faction.participants)
			{
				AI_faction.AddCharacterToBattleOrder(participant, null, tbbeActionType.Attack);
				
				AI_faction.LastBattleOrderEntry.SelectTarget();
				AI_faction.LastBattleOrderEntry.AssignAttack( AI_faction.LastBattleOrderEntry.RandomAttack() );
				participant.attack_state = tbbeAttackState.Ready;
			}
			AI_faction.OnFactionConfigured(AI_faction);
		}
		
		void OnBattleFieldStateChangedToBattling()
		{
			
		}
		
		void OnBattleFieldStateChangedToResultsScreen()
		{
			battle_field.PlayBackgroundMusic( battle_field.results_screen_music, battle_field.results_screen_volume);
		}
		
		
		void OnBattleFieldStateChangedToVictory()
		{
			battle_field.PlayBackgroundMusic( battle_field.victory_music, battle_field.end_of_game_volume);
			opening_top.msg_icon = victory_image;
			
			opening_top.onDone = OnVictoryDone;
			opening_top.StartIntro();
		}
		
		
		void OnBattleFieldStateChangedToDefeat()
		{
			battle_field.PlayBackgroundMusic( battle_field.defeat_music, battle_field.end_of_game_volume);
			opening_top.msg_icon = defeat_image;
			
			opening_top.onDone = OnDefeatDone;
			opening_top.StartIntro();
		}
		
		
	}
}
