using UnityEngine;
using System.Collections;

namespace MBS {
	/// <summary>
	/// Contains info relating to spells
	/// </summary>
	public class tbbSpellInfo : tbbAttackInfo {
		
		/// <summary>
		/// The name of the animation file to apply to the cinematic camera when this spell is cast
		/// </summary>
		public string	cinematic_animation_camera;
		
		/// <summary>
		/// The name of an animation to play on the character while he is casting this spell. This will play before the attack animation plays
		/// </summary>
		public string	cinematic_animation_character;
		
		/// <summary>
		/// An audio clip to play when this spell is cast
		/// </summary>
		public AudioClip	cinematic_audio;
		
		/// <summary>
		/// The prefab to instantiate when this spell is cast
		/// </summary>
		public Transform	special_effect;
		
		/// <summary>
		/// When this spell is cast, you can delay the spawning of the special effect to make it match the animation, if need be
		/// </summary>
		public float 	magic_FX_spawn_delay;
		
		/// <summary>
		/// How much MP the caster will require to cast this spell.
		/// The caster's MP will be depleted by this amount
		/// </summary>
		public int mp_required = 1;
	}
}
