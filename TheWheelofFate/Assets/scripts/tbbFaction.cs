using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// One of the most important classes in this kit, it does not offer a lot to configure in the inspector
	/// but it is one of the driving forces behind this kit.
	/// 
	/// Your battlefield object must always have exactly two of these components on it. One for the player
	/// faction and one for the enemy faction.
	/// 
	/// The component is in charge of spawning the players that belong to this faction, managing the order in which
	/// they attack, determining wether target selection is done via AI or require human intervention, deals with
	/// game over functions, character dying management as well as tinting the tiles to indicate player selection
	/// </summary>
	public class tbbFaction : MonoBehaviour, tbbIBattleBase
	{
		
		/// <summary>
		/// When a character's timer has recharged, tell the faction about it via this
		///	Not currently in use in this version of the kit... Will be used by RealTime mode
		/// </summary>
		System.Action<tbbPlayerInfo>
			onTimerRecharged;				
		
		/// <summary>
		/// Specify wether this faction is AI or Human controlled
		/// </summary>
		public tbbeControlMethod
			Intelligence;
		
		/// <summary>
		/// The name of this faction. This is basically just for your own information.
		/// For the developer, this can be seen in the Hierachy during play.
		/// Feel free to ignore 
		/// </summary>
		public string 
			faction_name;	// name of this faction
		
		public Texture2D faction_banner { get {return battlefield_side == tbbeBattlefieldSide.Lower ? tbbBattleField.Instance.bottom_faction_banner : tbbBattleField.Instance.top_faction_banner;} }
		
		/// <summary>
		/// Instead of relying on comparing a faction object to another faction object to determine which side of the board
		/// this faction is fighting on, use this instead...
		/// </summary>
		public tbbeBattlefieldSide
			battlefield_side;
		
		/// <summary>
		/// Specifies the characters that can be spawned on this faction. For the player faction,
		/// include all characters and spawn all. For the enemy faction, select all enemies of a
		/// particular level/ area / something that can spawn on this faction and then spawn a
		/// random subset thereof. See <c>SpawnCombatants</c>
		/// </summary>
		public tbbPlayerInfo[]
		party;
		
		
		/// <summary>
		/// Triggered when a target is selected during character selection mode
		/// </summary>
		public System.Action<tbbBattleOrder>	onBattleOrderTargetSelected;
		
		/// <summary>
		/// Triggered when all phases of configuration are complete and controll can be
		/// passed back to selecting the next character to go into battle
		/// </summary>
		public System.Action<tbbBattleOrder>	onBattleOrderEntryConfigured;
		
		/// <summary>
		/// Triggered after a character has finished it's attack and returned to home base
		/// </summary>
		public System.Action<tbbBattleOrder>	onBattleOrderEntryTurnCompleted;
		
		
		/// <summary>
		/// Triggered in PerFaction game mode after all characters are configured and ready to do battle 
		/// </summary>
		public System.Action<tbbFaction>	onFactionConfigured;
		/// <summary>
		/// Triggered when all characters have had their turn to do battle and all characters
		/// are either dead or on home base. This will trigger the switching state on the tbbBattleField instance
		/// </summary>
		public System.Action<tbbFaction>	onFactionTurnDone;
		
		/// <summary>
		/// This list contains the configured characters that will go into battle in the order that they will go into batte
		/// </summary>
		[System.NonSerialized]
		public List<tbbBattleOrder>
			battle_order;
		
		
		/// <summary>
		/// When the battle starts, will the characters have their backs turned to the enemy?
		/// </summary>
		[System.NonSerialized] public bool	facing_away = false;
		/// <summary>
		/// Mostly an internal field. This is supposed to aways be true for the top faction so the tile usage is calculated correctly.
		/// </summary>
		[System.NonSerialized] public bool	inverted;
		
		int Cols 		{ get { return tbbBattleField.Instance.grid_width; } } 
		int Rows 		{ get { return tbbBattleField.Instance.grid_height; } } 
		
		/// <summary>
		/// These are the party members that have been selected to participate in this round and who are still alive
		/// </summary>
		[System.NonSerialized]
		public List<tbbPlayerInfo> 
			participants	= null;
		
		/// <summary>
		/// This returns how many characters have already been selected to go into battle this turn
		/// </summary>
		public int 				CountBattleOrderEntries	{ get { return (null == battle_order ) ? 0	  : battle_order.Count	; } }
		/// <summary>
		/// This points to the last character selected to go into battle this round. As such, since a character is removed from this
		/// listince his attack is complete and a character is only added once you have selected it, it is safe to assume that
		/// LastBattleOrderEntry is the character that is being configured. Just make sure it's not null before you use it.
		/// </summary>
		/// <value>Either <c>NULL</c> or the character currently being configured</value>
		public tbbBattleOrder	LastBattleOrderEntry	{ get { return (null == battle_order || CountBattleOrderEntries == 0 ) ? null : battle_order[CountBattleOrderEntries-1]; }}
		
		bool AllCharactersConfigured 
		{
			get
			{
				if (CountBattleOrderEntries < participants.Count)
					return false;
				
				foreach(tbbBattleOrder entry in battle_order)
					if (entry.attack_state == tbbeAttackState.Config)
						return false;
				return true;
			}
		}
		
		/// <summary>
		/// Checks to see if all characters have had their attack turns.
		/// </summary>
		/// <value><c>true</c> if all characters attacked; otherwise, <c>false</c>.</value>
		public bool AllCharactersAttacked 
		{
			get
			{
				if (CountBattleOrderEntries < participants.Count)
					return false;
				
				foreach(tbbBattleOrder entry in battle_order)
					if (entry.attack_state != tbbeAttackState.Done)
						return false;
				return true;
			}
		}
		
		public tbbGrid grid { get { return tbbBattleField.Instance.grid; } }
		
		void Start()
		{
			onBattleOrderTargetSelected		+= __onBattleOrderTargetSelected;
			onBattleOrderEntryConfigured	+= __onBattleOrderEntryConfigured;
			onBattleOrderEntryTurnCompleted	+= __onBattleOrderEntryTurnCompleted;
			onFactionTurnDone				+= __onFactionTurnDone;
			onFactionConfigured				+= __onFactionConfigured;
			onTimerRecharged				+= __onTimerRecharged;
		}
		
		public void Initialize()
		{
			if (null == participants)
				participants = new List<tbbPlayerInfo>();
			else
				participants.Clear();
			
			if (faction_name == string.Empty)
			{
				faction_name  = tbbBattleField.me == this ? "Player" : "Enemy";
				faction_name += "Faction";
			}
		}
		
		/// <summary>
		/// Resets the battle order list to empty to ensure all characters are available for battle again
		/// </summary>
		public void ClearBattleOrder()
		{
			if (null == battle_order) battle_order = new List<tbbBattleOrder>();
			foreach(tbbBattleOrder entry in battle_order)
				Destroy(entry.gameObject);
			battle_order.Clear();
		}
		
		/// <summary>
		/// Tests to see if the specific character has already been configured and thus not available for selection
		/// </summary>
		/// <returns><c>true</c>, if configured and waiting to attack, <c>false</c> otherwise.</returns>
		/// <param name="character">The character to test</param>
		public bool CharacterAwaitingAttackTurn(tbbPlayerInfo character)
		{
			if (CountBattleOrderEntries > 0)
				foreach ( tbbBattleOrder entry in battle_order )
					if (entry.character == character && entry.attack_state == tbbeAttackState.Ready)
						return true;
			return false;
		}
		
		/// <summary>
		/// Checks to see if a specified character is in the battle order list
		/// </summary>
		/// <returns><c>true</c>, if the charactered is in battle order list, <c>false</c> otherwise.</returns>
		/// <param name="character">Character.</param>
		public bool CharacterInBattleOrderList(tbbPlayerInfo character)
		{
			return SpecifiedBattleOrderEntryi(character) >= 0;
		}
		
		/// <summary>
		/// Locate and return the configuration data of a specific character
		/// </summary>
		/// <returns>The battle order entry of the specified character if found, <c>null</c> otherwise</returns>
		/// <param name="character">The character to search for</param>
		public tbbBattleOrder SpecifiedBattleOrderEntry(tbbPlayerInfo character)
		{
			int i = SpecifiedBattleOrderEntryi(character);
			return i < 0 ? null : battle_order[i];
		}
		
		/// <summary>
		/// Find what order a specific character has in the battle order array
		/// </summary>
		/// <returns>The battle order index if the character is in the list, -1 otherwise</returns>
		/// <param name="character">The character to search for</param>
		public int SpecifiedBattleOrderEntryi(tbbPlayerInfo character)
		{
			if (CountBattleOrderEntries > 0)
				for (int index = 0; index < battle_order.Count; index++ )
					if (battle_order[index].character.CharacterID == character.CharacterID)
						return index;
			return -1;
		}
		
		#region public event triggers
		/// <summary>
		/// This event responder is called whenever a target is chosen
		/// It gets called for every character so make sure to test for the right one
		/// </summary>
		/// <param name="entry">The tbbBattleOrder entry you will be working on. Test for the right one!</param>
		public void OnBattleOrderTargetSelected(tbbBattleOrder entry)
		{
			if ( null != onBattleOrderTargetSelected && null != entry)
				onBattleOrderTargetSelected(entry);
		}
		
		
		/// <summary>
		/// This event responder is called whenever a character is fully configred and ready to go into battle
		/// It gets called for every character so make sure to test for the right one
		/// </summary>
		/// <param name="entry">The tbbBattleOrder entry you will be working on. Test for the right one!</param>
		public void OnBattleOrderEntryConfigured(tbbBattleOrder entry)
		{
			if ( null != onBattleOrderEntryConfigured && null != entry)
				onBattleOrderEntryConfigured(entry);
		}
		
		/// <summary>
		/// This event responder is called whenever a character has complted his attack and is back on his home tile
		/// It gets called for every character so make sure to test for the right one
		/// </summary>
		/// <param name="entry">The tbbBattleOrder entry you will be working on. Test for the right one!</param>
		public void OnBattleOrderEntryTurnCompleted(tbbBattleOrder entry)
		{
			if ( null != onBattleOrderEntryTurnCompleted && null != entry)
				onBattleOrderEntryTurnCompleted(entry);
		}
		
		
		/// <summary>
		/// This event responder is called when the entire faction has done their attack and the turn is over
		/// </summary>
		/// <param name="entry">This param should always point to itself</param>
		public void OnFactionTurnDone(tbbFaction faction)
		{
			if ( null != onFactionTurnDone && null != faction)
				onFactionTurnDone(faction);
		}
		
		/// <summary>
		/// This event responder is called whenever the entire faction is configured and rady to start battle
		/// </summary>
		/// <param name="entry">This param should always point to itself</param>
		public void OnFactionConfigured(tbbFaction faction)
		{
			if ( null != onFactionConfigured && null != faction)
			{
				onFactionConfigured(faction);
			}
		}
		#endregion
		
		/// <summary>
		/// If there are no characters currently busy attacking, make this character attack.
		/// </summary>
		/// <returns><c>true</c>, if character was sent into battle, <c>false</c> otherwise.</returns>
		/// <param name="entry">The tbbBattleOrder object for the character we want to send into battle</param>
		public bool SendCharacterIntoBattle(tbbBattleOrder entry)
		{
			bool battle_ready = entry.attack_state == tbbeAttackState.Ready;
			
			if (battle_ready)
			{
				foreach(tbbBattleOrder comrade in battle_order)
					if (comrade != entry && comrade.attack_state == tbbeAttackState.Fighting)
						battle_ready = false;
			}
			
			if  (battle_ready)
				entry.BeginAttack(tbbAttackPhase.OpeningCinematic, tbbeAttackState.Ready);
			
			return battle_ready;
		}
		
		/// <summary>
		/// This will probe the list of configured characters for the first configured character
		/// that has not yet had his turn this round and attempt to send him into battle. Since the
		/// <c>SendCharacterIntoBattle</c> function will deny the character his attack if there is
		/// already a character attacking, te combination of these two functions work as a queueing system.
		/// </summary>
		public void StartBattleQueue()
		{
			foreach(tbbBattleOrder entry in battle_order)
			{
				if (entry.character.ready_for_attack)
				{
					if (SendCharacterIntoBattle(entry))
						return;
				}
			}
		}
		
		void __onCharacterDied(tbbPlayerInfo character)
		{
			for ( int i = 0; i < participants.Count; i++)
				if (participants[i] == character)
			{
				participants.Remove(character);
				
				if (tbbBattleField.opponent.participants.Count == 0)
					tbbBattleField.Instance.doEndMatch();
				return;
			}
		}
		
		/// <summary>
		/// Spawn all or only a subset of the characters to make a slightly more randomized game
		/// </summary>
		/// <param name="capped">Spawn all or cap the max number of participants?</param>
		/// <param name="limit">If <c>capped</c>, what is max amount of characters to spawn?</param>
		/// <param name="spawn_all">If <c>capped</c>, should max amount of characters be spawned, as set by <c>limit</c>?</param>
		/// <param name="minimum">If <c>spawn_all</c> is false, what is the minimum amount of characters to spawn?</param>
		public void SpawnCombatants()				{ SpawnCombatants(false, 0, false, 1);		}
		public void SpawnCombatants(bool spawn_all) { SpawnCombatants(false, 0, spawn_all, 1);	}
		public void SpawnCombatants(bool capped, int limit, bool spawn_all, int minimum)
		{
			if (null == party || party.Length == 0)
			{
				StatusMessage.Message = "There are no soldiers in this faction!";
				Debug.LogError("Cannot spawn characters. No characters defined!");
				return;
			}
			
			int 
				character_count = 0;
			
			List<tbbPlayerInfo> participants = new List<tbbPlayerInfo>();
			
			//if the limit is more than the total amount of characters in the party, 
			//this could cause an infinite loop. Also, a limit of 0? Seriously?
			//override some values to make them make more sense...
			if (capped)
			{
				if (limit < 1) limit = 1;
				if (limit > party.Length) limit = party.Length;
				
				if (!spawn_all)
				{
					if (minimum > party.Length)
						minimum = party.Length;
					if (minimum < 1)
						minimum = 1;
					if (minimum > limit)
						minimum = 1;
				}
				
				//if not spawning all characters, pick a random number to spawn
				if (limit > 1 && !spawn_all && limit != minimum)
					limit = Random.Range(minimum,limit+1);
			}
			
			//either load a random selection of characters
			if (capped)
			{
				List<tbbPlayerInfo> cloud = new List<tbbPlayerInfo>();
				foreach(tbbPlayerInfo member in party)
					cloud.Add(member);
				
				do
				{
					if (cloud.Count == 1)
					{
						participants.Add(cloud[0]);
						break;
					}
					int index = Random.Range(0, cloud.Count);
					participants.Add(cloud[index]);
					cloud.RemoveAt(index);
				} while (participants.Count < limit);
			}
			//or load all party members...
			else
			{
				foreach(tbbPlayerInfo member in party)
					participants.Add(member);
			}
			
			//and finally, spawn all battle participants...
			foreach (tbbPlayerInfo character in participants)
			{
				//attempt to place all characters at random locations
				//only attempt 100 locations per character. If the current character
				//cannot be placed, skip it...
				bool found = false;
				int	counter = 0;
				Vector2
					location = Vector2.zero,
					space = character.tiles_required,
					not_found = new Vector2(-1,-1);
				
				if (space.x == 0) space.x = 1;
				if (space.y == 0) space.y = 1;
				
				while (!found && counter++ < 100)
				{
					location = grid.FindRandomLocation(space, battlefield_side);
					found = location != not_found;
				}
				if (found) SpawnCharacter(character, location, space);
				
				if (capped && ++character_count >= limit)
					break;
			}
		}
		
		/// <summary>
		/// Create an entry in the battle order array so we know which characters fight in which order.
		/// </summary>
		/// <param name="character">The character you want to send into battle</param>
		/// <param name="target">If you know who to target, specify that here, <c>null</c> otherwise</param>
		/// <param name="action">What type of attack will this be? Melee, magic, other...?</param>
		public void AddCharacterToBattleOrder(tbbPlayerInfo character, tbbPlayerInfo target, tbbeActionType action)
		{
			if (null == battle_order)
				battle_order = new List<tbbBattleOrder>();
			
			GameObject go = new GameObject("BattleOrderEntry"+ CountBattleOrderEntries);
			tbbBattleOrder bo = go.AddComponent<tbbBattleOrder>();
			bo.Construct(character, target, action);
			bo.Inverted = inverted;
			bo.intelligence = Intelligence;
			bo.transform.parent = transform;
			bo.onTargetSelected += onBattleOrderTargetSelected;
			battle_order.Add(bo);
			
			//			if ( tbbBattleField.Instance.battle_type != tbbeBattleType.Realtime)
			//			{
			LastBattleOrderEntry.onTargetSelected	+= onBattleOrderEntryConfigured;
			LastBattleOrderEntry.onTurnCompleted	+= onBattleOrderEntryTurnCompleted;
			//			}
			
			LastBattleOrderEntry.character.onTimerRecharged += onTimerRecharged;
			
		}
		
		/// <summary>
		/// Removes the character from battle_order list and enables the character to be selected for configuration again
		/// </summary>
		/// <returns><c>true</c>, if character was removed from the battle_order list, <c>false</c> otherwise.</returns>
		/// <param name="index">Index of the character in the battle_order list</param>
		public bool RemoveCharacterFromBattleOrderi(int index)
		{
			if (null == battle_order || index >= battle_order.Count || index < 0)
				return false;
			
			battle_order.RemoveAt(index);
			return true;
		}
		
		/// <summary>
		/// Removes the last character from the battle_order list.
		/// </summary>
		public void RemoveLastCharacterFromBattleOrder() { RemoveCharacterFromBattleOrderi(battle_order.Count - 1); }
		
		/// <summary>
		/// Used during the creation of the battlefield. Spcifies wether this faction should be "turned around" to face
		/// the other faction. This affects the selection of adjacent tiles when a character requires more than one tile.
		/// 
		/// This also rotates the characters spawned on this tile by 180 degrees
		/// </summary>
		/// <param name="inverted">Example: If a character requires 2 tiles horizontally and 2 tiles deep, if <c>false</c> the tile to the left
		/// of the character needs to be empty as well as the tiles above and to the left it in order to be placed in that spot.
		/// When not inverted, the tile to the right and the tiles below and to the right of the character needs to be vacant
		/// before the character will be assigned that spot.</param>
		/// <param name="face_away">If set to <c>true</c> rotate the characters to face the other way</param>
		public void SetInverted(bool inverted) { SetInverted ( inverted, false); }
		public void SetInverted(bool inverted, bool face_away)
		{
			this.inverted = inverted;
			this.facing_away = face_away;
		}
		
		/// <summary>
		/// Spawn the character prefab onto the selected tiles and rotate as required
		/// </summary>
		/// <returns><c>true</c>, if character was spawned, <c>false</c> if there were no open tiles this character could fit into</returns>
		/// <param name="_character">The character to spawn</param>
		/// <param name="location">The starting tile index into the faction's array</param>
		/// <param name="space">How many tiles does this character need?</param>
		public bool SpawnCharacter(tbbPlayerInfo _character, Vector2 location, Vector2 space)
		{
			tbbPlayerInfo character = (tbbPlayerInfo)Instantiate(_character);
			character.transform.name = _character.character_name + "_" + this.faction_name;
			
			PositionCharacter(character, location, space);
			character.transform.localRotation = grid.camp.transform.localRotation;
			
			character.onDied += __onCharacterDied;
			
			if (inverted)
				character.transform.Rotate(0,180,0);
			
			if (facing_away)
				character.transform.Rotate(0,180,0);
			
			character.facing_away = facing_away;
			
			participants.Add(character);
			
			return true;
		}
		
		public void PositionCharacter(tbbPlayerInfo character, Vector2 location, Vector2 space)
		{
			character.transform.position = grid.Center(location, space);
			character.transform.parent = grid.tiles[(int)location.x, (int)location.y].transform;
			character.tile_index = location;
			grid.SetOccupation(location,space,character);
		}
		
		
		#region default event handlers
		
		
		void __onBattleOrderTargetSelected(tbbBattleOrder entry)
		{
			entry.attack_state = tbbeAttackState.Ready;
		}
		
		
		void __onBattleOrderEntryConfigured(tbbBattleOrder entry)
		{
			if (entry != LastBattleOrderEntry)
				return;
			
			switch (tbbBattleField.Instance.battle_type)
			{
				//			case tbbeBattleType.Realtime:
				//				StartBattleQueue();
				//				break;
				
			case tbbeBattleType.PerFaction:
				if (AllCharactersConfigured)
					OnFactionConfigured(this);
				break;
				
			case tbbeBattleType.Immediate:
				StartBattleQueue();
				break;
			}
			
			tbbGUICursor.Instance.Display = (null != battle_order && CountBattleOrderEntries < participants.Count && !AllCharactersConfigured);
		}
		
		void __onBattleOrderEntryTurnCompleted(tbbBattleOrder entry)
		{
			if (entry.character.needs_recharge)
				entry.character.ResetTimer();
			
			if (AllCharactersAttacked)
				OnFactionTurnDone(this);
			else
				StartBattleQueue();
		}
		
		void __onFactionTurnDone(tbbFaction faction)
		{
			for(int i = battle_order.Count-1; i >= 0; --i)
			{
				//				if (tbbBattleField.Instance.battle_type != tbbeBattleType.Realtime)
				//				{
				Destroy (battle_order[i].gameObject);
				battle_order.RemoveAt(i);
				//				}
			}
		}
		
		void __onFactionConfigured(tbbFaction faction)
		{
			grid.ResetTileModes();
			StartBattleQueue();
		}
		
		//when the timer state has changed on the tbbPlayerInfo instance,
		//it will call OnTimerChanged on it's faction which will call this
		//to enqueue the character into the battle order...
		void __onTimerRecharged(tbbPlayerInfo character)
		{
			tbbBattleOrder entry = SpecifiedBattleOrderEntry(character);
			if (null == entry)
			{
				Debug.Log ("Null!!!");
				return;
			}
			
			SendCharacterIntoBattle (entry);
		}
		#endregion
	}
}
