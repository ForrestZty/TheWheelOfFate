using UnityEngine;
using System.Collections;

namespace MBS {
	/// <summary>
	/// Contains info relating to the attacks system. Add this component to an empty gameobject to create a new attack.
	/// </summary>
	public class tbbAttackInfo : tbbDamage {
		
		/// <summary>
		/// Depending on what functionality is added to the kit, this would specify what type of attack this is.
		/// Is this a nromal attack or is this a magic attack or is this a flee attempt or will this turn let
		/// the user select an item from inventory? By default there is only melee and magic but more will be added in time...
		/// </summary>
		public tbbeActionType
			attack_type;
		
		/// <summary>
		/// From where will this attack take place? Will the character attack from his own base tile or will he run to the
		/// enemy before attacking or attack from the neutral area between the factions? This setting is set globally in the
		/// tbbBattleField component so by default this is set to 'Default' which assumes the value from there. 
		/// If you set any other value on here, it will override the global setting for this attack only. 
		/// </summary>
		public tbbeAttackModeExt 
			attack_mode = tbbeAttackModeExt.Default;	//fight from current location or move closer?
		
		tbbeAttackMode _attackmode;
		
		/// <summary>
		/// Use this property to automatically convert the attack_mode variable from it's enum type to the battlefield's enum type
		/// </summary>
		/// <value>The attack mode.</value>
		public tbbeAttackMode AttackMode { get { return attack_mode == tbbeAttackModeExt.Default ? DetermineAttackType() : _attackmode;} }
		
		
		/// <summary>
		/// A name to identify this attack by. Currently not used by anything but there none the less
		/// </summary>
		public string attack_name;
		/// <summary>
		/// The name of the animation to play on the character that performs this attack. You need to make sure
		/// that the CharacterController attached to the character has an animation by this name.
		/// This filed is case sensitive.
		/// </summary>
		public string attack_animation_name;
		
		/// <summary>
		/// An audio file to play when this animation starts. Ideal for things like "Aiiija" yells and stuff like that...
		/// </summary>
		public AudioClip
			attack_audio;
		
		/// <summary>
		/// An object to spawn when this attack takes place. This could be anything from a flash of light to a demon summon.
		/// It can be anything you want it to be as this is completely independent of the actual attack but might just add
		/// that extra layer of OOMPH to th attack
		/// </summary>
		public Transform
			attack_special_effect;
		
		/// <summary>
		/// In case you want to spawn an object as a special effect but not spawn it immediately when the attack starts.
		/// A fireball is a good example. You want to charge the move first then spawn the fireball when the character's
		/// arms move forward. Simply test the animation and see how long into the animation would be a good time to
		/// spawn the prefab and then add that value here
		/// </summary>
		public float 
			attack_FX_spawn_delay;
		
		
		/// <summary>
		/// Should the prefab spawned during the attack be spawned at the player or the target's location?
		/// I.e. is the prefab a beam of light that should surround the player or n block of ice that should encase the target?
		/// </summary>
		public tbbeSpawnOrigin	attack_FX_origin;
		/// <summary>
		/// Should the prefab spawned during the spell be spawned at the player or the target's location?
		/// I.e. is the prefab a beam of light that should surround the player or n block of ice that should encase the target?
		/// </summary>
		public tbbeSpawnOrigin	magic_FX_origin;
		
		tbbeAttackMode DetermineAttackType()
		{
			if (attack_mode != tbbeAttackModeExt.Default)
				return (tbbeAttackMode)((int)attack_mode);
			attack_mode = (tbbeAttackModeExt)((int)tbbBattleField.Instance.attack_mode);
			_attackmode = tbbBattleField.Instance.attack_mode;
			return _attackmode;
		}
		
		public void Start()
		{
			DetermineAttackType();
		}
	}
}
