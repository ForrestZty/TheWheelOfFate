using UnityEngine;
using System.Collections;

namespace MBS {
	/// <summary>
	/// This component is still a work in progress.
	/// It's intent is to configure the battlefield prior to loading a new scene in which the battlefield is actually loaded.
	/// For this reason, an object that has thsi component on should be loaded OUTSIDE the scene in which the battlefield is
	/// created and set to DontDestroyOnLoad so that the scene can simply be reloaded in order to do a rematch in the event of a fail.
	/// 
	/// You need to add your own functionality for when to delete this gameobject when you are done with it. 
	/// </summary>
	public class tbbBattleLauncher : MonoBehaviour {
		
		//Challengers are at the bottom of the screen
		public enum tbbeInvertedStart {None, Bottom, Top}
		
		/// <summary>
		/// When the match begins, should:
		///  -(a) None: The players face each other
		///  -(b) Challenger: The BottomFaction characters have their backs turned to the enemy
		///  -(c) Defender: The TopFaction characters have their backs turned to the enemy
		/// </summary>
		public tbbeInvertedStart 
			invert_starting_direction;
		
		/// <summary>
		/// Indicates which faction will get to play first
		/// </summary>
		public tbbeBattlefieldSide
			StartingFaction = tbbeBattlefieldSide.Lower;
		
		/// <summary>
		/// This is more for development purposes than anything else. If you have this selected
		/// it will spawn the battlefield right then and there. If not, you can work on the object
		/// in a scene and hit play in the scene without it instantiating the battlefield.
		/// </summary>
		public bool
			auto_load_in_editor = true;
		
		/// <summary>
		/// Holds a reference to which battlefield prefab you wish to instantiate when the new
		/// scene has finished loading.
		/// </summary>
		public tbbBattleField
			battle_field_prefab;
		
		
		/// <summary>
		/// The graphic for the faction figting in the top part of the battlefield to display during the "Player vs OtherPlayer" screen
		/// </summary>
		public Texture2D	top_faction_name_img;
		/// <summary>
		/// The graphic for the faction figting in the bottom part of the battlefield to display during the "Player vs OtherPlayer" screen
		/// </summary>
		public Texture2D	bottom_faction_name_img;
		
		
		/// <summary>
		/// Here you get to specify the characters that will battle in the bottom part of the battlefield
		/// </summary>
		public tbbPlayerInfo[]	bottom_faction_party_members;
		/// <summary>
		/// Here you get to specify the characters that will battle in the top part of the battlefield
		/// </summary>
		public tbbPlayerInfo[]	top_faction_party_members;
		
		/// <summary>
		/// This is just to try and prevent double loading. While working in the scene in the editor
		/// the OnLevelWasLoaded doesn't fire so I load it in Start. At runtime, though, I want to
		/// call it after the level was loaded and skip loading during start. To this end I created
		/// the auto_load_in_editor bool value but in case you forget to change it when you exit the
		/// scene, just to prevent the battliefield spawning twice and giving you errors since there 
		/// can be only one instance of it, I do this as an extra check to play it safe...
		/// </summary>
		bool
			did_load = false;
		
		void Start()
		{
			//OnLevelWasLoaded does not fire in the editor so
			if (Application.isEditor && auto_load_in_editor)
			{
				LaunchBattleSystem();
			}
		}
		
		void OnLevelWasLoaded(int level)
		{
			if (!auto_load_in_editor)
				LaunchBattleSystem();
		}
		
		/// <summary>
		/// Launches the battlefield at this object's position
		/// </summary>
		public void LaunchBattleSystem()
		{
			LaunchBattleSystem(transform.position, transform.rotation);
		}
		
		/// <summary>
		/// Overloaded function to compensate for Unity's lack of default parameters when using namespaces
		/// Launches the battlefield at the specified location
		/// </summary>
		/// <param name="location">Location.</param>
		public void LaunchBattleSystem(Vector3 location)
		{
			LaunchBattleSystem(location, transform.rotation);
		}
		
		/// <summary>
		/// Overloaded function to compensate for Unity's lack of default parameters when using namespaces
		/// Launches the battlefield at the specified location
		/// </summary>
		/// <param name="location">Location.</param>
		/// <param name="rotation">Rotation.</param>
		public void LaunchBattleSystem(Vector3 location, Quaternion rotation)
		{
			if (did_load)
				return;
			else
				did_load = true;
			
			tbbBattleField bf = (tbbBattleField)Instantiate(battle_field_prefab, location, rotation);
			
			bf.bottom_faction_banner = bottom_faction_name_img;
			bf.top_faction_banner = top_faction_name_img;
			bf.top_starts_inverted = invert_starting_direction == tbbeInvertedStart.Top;
			bf.bottom_starts_inverted = invert_starting_direction == tbbeInvertedStart.Bottom;
			bf.StartingFaction = StartingFaction;
			bf.onPreCharacterLoad += SwopCharacters;
			bf.InitializeBattleSystem();
			
			if (bf.battle_music)
			{
				bf.PlayBackgroundMusic(bf.battle_music, 0.7f);
			}
		}
		
		void SwopCharacters(tbbBattleField battlefield)
		{
			battlefield.TopFaction.party = top_faction_party_members;
			battlefield.BottomFaction.party = bottom_faction_party_members;
		}
	}
}
