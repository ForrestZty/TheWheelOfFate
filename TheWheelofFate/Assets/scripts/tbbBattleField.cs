using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	
	/// <summary>
	/// This is the main component in this kit. All other classes either go on to the same object this component is on
	/// or goes on to objects that are a child of the object this component is on but either way, this component holds
	/// info vital to the proper functioning of this system.
	/// 
	/// The most important element of this component is it's reference to the player and enemy factions and it's ability 
	/// to distinguish between the two and switch from the one to the other at the end of either's turn.
	/// 
	/// A smaller, but just as important setting contained in this component is the battle type which determines the 
	/// character selection process to use and imposes certain freedoms or limitations thereon.
	/// 
	/// Also, this component is the one that actually generates the battlefield, the factions, spawns the characters and
	/// tells all other components when it's time to do what. 
	/// </summary>
	public class tbbBattleField : MonoBehaviour {
		
		static tbbBattleField _instance;
		/// <summary>
		/// Allows for all scripts to have direct access to this component without having to manually setup references
		/// </summary>
		/// <value>The instance.</value>
		static public tbbBattleField Instance 
		{
			get
			{
				if (null == _instance)
					_instance = GameObject.FindObjectOfType<tbbBattleField>();
				return _instance;
			}
		}
		
		
		/// <summary>
		/// Allows for direct access to whichever of the two factions is the currently active faction
		/// </summary>
		static public tbbFaction	active_faction;
		
		/// <summary>
		/// A simple way to identify wether you are the active player. if (active_faction == me)
		/// </summary>
		static public tbbFaction	me;	
		
		
		/// <summary>
		/// This event fires just before the characters are loaded.
		/// Currently used by tbbBattleLauncher to swop out the characters
		/// </summary>
		public System.Action<tbbBattleField>	onPreCharacterLoad;

		/// <summary>
		/// This event fires just after all the characters have spawned.
		/// This is the ideal place to apply any saved status mods to the characters pre-battle
		/// </summary>
		public System.Action<tbbBattleField>	onCharactersLoaded;
		
		/// <summary>
		/// Allows for a quick and easy way to identify the non-active faction.
		/// </summary>
		/// <value>The opponent.</value>
		static public tbbFaction  opponent { get { return active_faction == Instance.TopFaction ? Instance.BottomFaction : Instance.TopFaction; } }
		
		
		/// <summary>
		/// Allows for direct access to the current state of the battlefield. This allows any component to instantly
		/// know wether or not they should do any processing at all, based on the state's requirements
		/// </summary>
		static public mbsStateMachine<tbbeBattleState>
			game_state;
		
		/// <summary>
		/// Specifies the overall look and layout of the grid
		/// </summary>
		public tbbGridModes
			grid_style;
		
		
		/// <summary>
		/// How many tiles wide is this battlefield?
		/// </summary>
		public int	grid_width	= 3;

		/// <summary>
		/// How man tiles deep is this battlefield?
		/// </summary>
		public int	grid_height = 2;

		/// <summary>
		/// The minimum amount of enemies to spawn
		/// </summary>
		public int	min_enemies = 1;

		/// <summary>
		/// When the list of enemies that may spawn for the faction is very large, this sets the limit for how many to actually spawn
		/// </summary>
		public int	max_enemies = 4;
		
		/// <summary>
		/// The total width of the battlefield in world units taking into account tile size and spacing
		/// </summary>
		public float GridWidth	{ get { return (grid_width * (tile_size + tile_margin)) - tile_margin; } } 
		/// <summary>
		/// The total depth of a faction's size in world units taking into account tile size and spacing
		/// </summary>
		public float GridHeight { get { return (grid_height * (tile_size + tile_margin)) - tile_margin; } } 
		
		
		/// <summary>
		/// The size of each tile in world units
		/// </summary>
		public float	tile_size	= 2f;

		/// <summary>
		/// An empty space between tiles
		/// </summary>
		public float	tile_margin = 0f;

		/// <summary>
		/// An area left open between the two factions, specified in world units
		/// </summary>
		public float	neutral_grounds = 2f;
		public float	starting_audio_volume = 0.7f;
		public float	end_of_game_volume = 0.7f;
		public float	results_screen_volume = 0.7f;
		
		
		/// <summary>
		/// Audio to play during battle
		/// </summary>
		public AudioClip	battle_music;
		/// <summary>
		/// Audio to play upon winning the match
		/// </summary>
		public AudioClip	victory_music;
		/// <summary>
		/// Audio to play upon loosing the match
		/// </summary>
		public AudioClip	defeat_music;
		/// <summary>
		/// Audio to play during the results screen
		/// </summary>
		public AudioClip	results_screen_music;
		
		
		/// <summary>
		/// If you are victorious, what scene should load next?
		/// </summary>
		public string 	level_to_load_on_victory = "MainGame";

		/// <summary>
		/// If you lost the match and do not want to retry, what scene should load next?
		/// </summary>
		public string 	level_to_load_on_quit = "MainMenu";
		
		
		/// <summary>
		/// Weather or not the bottom faction should start by facing away from the enemy. Default is false
		/// </summary>
		public bool	bottom_starts_inverted = false;

		/// <summary>
		/// Weather or not the top faction should start by facing the enemy. Since this faction will, by default, face
		/// the same direction as the other faction, this faction needs to be inverted in order to face them.
		/// For this reason, the default is true.
		/// </summary>
		public bool	top_starts_inverted = true;

		/// <summary>
		/// When spawning the enemy characters, should all enemies spawn or only a random subset?
		/// </summary>
		public bool	spawn_all_enemy_party_members = false;

		/// <summary>
		/// Should we go into the NewLocation state after selecting a character or go directly into action select?
		/// </summary>
		public bool	allow_repositioning = true;

		/// <summary>
		/// If we allow the character to be repositioned at the start of his/her turn, should we confine the character's
		/// movement to the side of the battlefield it was spawned on or allow it to navigate the entire grid?
		/// </summary>
		public bool	confine_movement = true;
		
		/// <summary>
		/// This determines how the game is played. More attack modes will be added in future (possibly) but
		/// for now this allows you to select from Immediate or PerFaction attack modes.
		/// 
		/// When attacking in Immediate mode, your character will attak as soon as you have assigned it a target and an action.
		/// Once the attack is configured, the character's turn is over until the end of the enemy's turn.
		/// 
		/// When attacking in PerFaction mode, you can go back and undo all the configurations you have made in the order that
		/// you made them startin from the last action you took, wether character selection or action selection, right up to
		/// the choice of which character will attack first. Only after all characters have been configured with a target and
		/// an action will the attack commence.
		/// </summary>
		public tbbeBattleType
			battle_type;
		
		/// <summary>
		/// Will the character attack from his base location, from the center of the battlefield or first run up to the enemy before attacking?
		/// This value will be passed along to all attacks and spells that have their attack mode set to Default.
		/// </summary>
		public tbbeAttackMode
			attack_mode = tbbeAttackMode.InPlace;
		
		/// <summary>
		/// Indicates which faction will get to play first
		/// </summary>
		public tbbeBattlefieldSide
			StartingFaction = tbbeBattlefieldSide.Lower;
		
		
		/// <summary>
		/// This is the image that will display during opening credits and during turn changes
		/// </summary>
		public Texture2D	top_faction_banner;

		/// <summary>
		/// This is the image that will display during opening credits and during turn changes
		/// </summary>
		public Texture2D	bottom_faction_banner;
		
		
		/// <summary>
		/// Holds a reference to the faction battling at the top of the screen
		/// </summary>
		[System.NonSerialized]public tbbFaction	TopFaction;

		/// <summary>
		/// Holds a reference to the faction battling at the bottom of the screen
		/// </summary>
		[System.NonSerialized]public tbbFaction	BottomFaction;
		
		/// <summary>
		/// Holds a reference to a prefab to spawn when a cinematic camera sequence is set to play.
		/// </summary>
		public tbbCinematicCamera
			cinema_cam;
		
		/// <summary>
		/// Holds a reference to the prefab to spawn for the tile. Characters will be placed on this
		/// tile's pivot point so if you want to load a 3D model like a hill or something, make sure that
		/// prefab's pivot is at the top of the mesh, not at the base.
		/// </summary>
		public tbbTile
			tile_prefab;
		
		/// <summary>
		/// A prefab to spawn if you enable character movement during turns
		/// </summary>
		public Transform
			tile_cursor;
		
		/// <summary>
		/// Holds a reference to an AudioSource somewhere in the scene. Ideally this will be one on the MainCamera
		/// </summary>
		public AudioSource 
			audio_source_reference;

		/// <summary>
		/// The actual grid. Allows access to functions like tinting the grid and determining wether an area is available for occupation.
		/// Also allows you to actually set the occupation of the tiles
		/// </summary>
		[System.NonSerialized] public tbbGrid
			grid;
		
		
		void DetermineTopAndBottomFactions()
		{
			tbbFaction[] factions = GetComponents<tbbFaction>();
			if (null == factions || factions.Length != 2)
			{
				Debug.LogError("Battlefield requires 2 factions!");
				return;
			}
			
			if (factions[0].battlefield_side == factions[1].battlefield_side)
			{
				if (factions[0].Intelligence == tbbeControlMethod.Human)
				{
					factions[0].battlefield_side = tbbeBattlefieldSide.Lower;
					factions[1].battlefield_side = tbbeBattlefieldSide.Upper;
				} else
				{
					factions[1].battlefield_side = tbbeBattlefieldSide.Lower;
					factions[0].battlefield_side = tbbeBattlefieldSide.Upper;
				}
			}
			BottomFaction = factions[0].battlefield_side == tbbeBattlefieldSide.Lower ? factions[0] : factions[1];
			TopFaction = factions[0] == BottomFaction ? factions[1] : factions[0];
		}
		
		tbbGrid CreateField(string name)
		{
			tbbGrid grid = ScriptableObject.CreateInstance<tbbGrid>();
			grid.PrepareField(name, transform);
			return grid;
		}
		
		/// <summary>
		/// This function starts the whole thing off... It is created as a separate function instead of being placed
		/// in the Start function so that the Launchers can call this when ready.
		/// 
		/// This function sets up the base states and kickstarts the battle system with the Intro1 state after
		/// configuring the rest of the system. This means, in order:
		/// - creating the faction's respective battle areas,
		/// - spawning the characters on their respective battle areas,
		/// - subscribing to both of the factions "onFactionDone" event
		/// - positioning the battlefield
		/// - spawning the combatants
		/// - and then telling all the other components that it is now ready for them to start listening to...
		/// </summary>
		public void InitializeBattleSystem()
		{
			PrepareGlobalStates();
			DetermineTopAndBottomFactions();
			
			TopFaction.Initialize();
			BottomFaction.Initialize();
			
			grid = CreateField("BattleArea");
			
			BottomFaction.SetInverted(false, bottom_starts_inverted);
			TopFaction.SetInverted(true, top_starts_inverted);
			
			BottomFaction.onFactionTurnDone += __onFactionTurnDone;
			TopFaction.onFactionTurnDone += __onFactionTurnDone;
			
			//do this the opposite of what was selected because the battle starts with a turn switch
			active_faction = StartingFaction == tbbeBattlefieldSide.Upper ? BottomFaction : TopFaction;
			
			BottomFaction.battlefield_side = tbbeBattlefieldSide.Lower;
			TopFaction.battlefield_side = tbbeBattlefieldSide.Upper;
			
			me = BottomFaction.Intelligence == tbbeControlMethod.Human ? BottomFaction : TopFaction;
			//if there is only one human controlled faction but the dev set the player to the AI faction, change to the human one
			if (me.Intelligence == tbbeControlMethod.AI)
			{
				if (TopFaction.Intelligence == tbbeControlMethod.Human)
					me = TopFaction;
				else
					if (BottomFaction.Intelligence == tbbeControlMethod.Human)
						me = BottomFaction;
			}
			
			if (null != onPreCharacterLoad) onPreCharacterLoad(this);
			
			//always spawn all of the player's party
			me.SpawnCombatants();
			tbbFaction enemy = TopFaction == me ? BottomFaction : TopFaction;
			
			//but spawn only a selection of characters from the enemy list if required.
			//Alternatively, if specified, load all characters
			if (spawn_all_enemy_party_members)
				enemy.SpawnCombatants(true);
			else
				enemy.SpawnCombatants(true, max_enemies, false, min_enemies);
			
			if (null != onCharactersLoaded) onCharactersLoaded(this);
			
			SendStateUpdateMessage();
			gameObject.BroadcastMessage("SetupReferences", SendMessageOptions.DontRequireReceiver);
			gameObject.BroadcastMessage("OnBattleSystemInitialized", SendMessageOptions.DontRequireReceiver);
			
			//make sure the game starts without any tiles selectd by default...
			active_faction.grid.ResetTileModes();
		}
		
		public void PlayBackgroundMusic(AudioClip clip) { PlayBackgroundMusic(clip, 0.7f);}
		public void PlayBackgroundMusic(AudioClip clip, float volume)
		{
			if (null == clip)
				return;
			
			if (null == audio_source_reference)
			{
				AudioListener al = GameObject.FindObjectOfType<AudioListener>();
				audio_source_reference = al.transform.GetComponentInChildren<AudioSource>();
				if (null == audio_source_reference)
				{
					Instance.audio_source_reference = GameObject.FindObjectOfType<AudioSource>();
					if (null == audio_source_reference)
					{
						audio_source_reference = al.gameObject.AddComponent<AudioSource>();
					}
				}
				
				if (null == audio_source_reference)
					return;
				
				audio_source_reference.Stop();
				audio_source_reference.clip = clip;
				audio_source_reference.loop = true;
				audio_source_reference.volume = volume;
				audio_source_reference.Play();
			}
		}
		
		/// <summary>
		/// This changes the state of the battle and makes sure all other components are aware of the new state
		/// See also: public enum tbbeBattleState
		/// </summary>
		/// <param name="state">State.</param>
		static public void ChangeToBattleState(tbbeBattleState state)
		{
			game_state.SetState(state);
			Instance.SendStateUpdateMessage();
		}
		
		void SendStateUpdateMessage()
		{
			string message = "OnBattleFieldStateChangedTo" + game_state.CurrentState.ToString();
			BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
		}
		
		void PrepareGlobalStates()
		{
			game_state = new mbsStateMachine<tbbeBattleState>();
			for( tbbeBattleState s = (tbbeBattleState)0; s < tbbeBattleState.Count; s++)
				game_state.AddState(s);
			
			game_state.SetState(tbbeBattleState.Intro1);
		}
		
		public	void doOnCharSelectStateChange(tbbeCharSelModes state)	{ BroadcastMessage("OnCharSelectStateChanged", state, SendMessageOptions.DontRequireReceiver); }
		public	void doOnCharSelectInitialized() 				{ BroadcastMessage("OnCharacterSelectInitialized", SendMessageOptions.DontRequireReceiver); }
		public	void doOnActionSelected(tbbeActionType action)	{ BroadcastMessage("OnActionSelected", action, SendMessageOptions.DontRequireReceiver); }
		public	void doOnCharacterSelected(int character)		{ BroadcastMessage("OnCharacterSelected", character, SendMessageOptions.DontRequireReceiver); } 
		
		public	void doEndMatch() 
		{
			tbbBattleField.ChangeToBattleState(active_faction == me ? tbbeBattleState.Victory : tbbeBattleState.Defeat);
			BroadcastMessage("OnMatchEnded", SendMessageOptions.DontRequireReceiver);
		}
		
		/// <summary>
		/// Listen to the factions to pick up on when their respective turns are done. Once it is done, change to the Switching state.
		/// This event responder does not use the included parameter
		/// </summary>
		/// <param name="faction">Faction.</param>
		void __onFactionTurnDone(tbbFaction faction)
		{
			if (opponent.participants.Count == 0)
			{
				if (active_faction == me)
					ChangeToBattleState(tbbeBattleState.Victory);
				else
					ChangeToBattleState(tbbeBattleState.Defeat);
			} 
			else
				ChangeToBattleState(tbbeBattleState.Switching);
		}
		
	}
	
}
