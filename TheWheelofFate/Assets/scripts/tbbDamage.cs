using UnityEngine;
using System.Collections;

namespace MBS {
	/// <summary>
	/// The tbbDamage class is in charge of actually dealing damage from one player to another.
	/// It cointains a virtual function that you can override to create your own damage calculation routines
	/// It is also in charge of spawning the graphical representations of the damage dealt.
	/// </summary>
	public class tbbDamage : MonoBehaviour {

		[System.Serializable]
		public class _delayed_damage
		{
			public float delay;
			public int damage;
		}


		/// <summary>
		/// Allows you to hook into each blow as it is dealt. This will pass in the amount of damage caused by the blow as it's parameter
		/// </summary>
		public System.Action<int>
			onDamageDealt;

		/// <summary>
		/// Allows you to hook into the event right after the final blow was dealt.
		/// </summary>
		public System.Action
			onAllDamageDealt;

		public tbbPlayerInfo
			/// <summary>
			/// This should be the player that casts this move
			/// </summary>
			attacker,
			/// <summary>
			/// This is the target of the attack and the one to take the damage.
			/// Currently my damage function only deals damage. At a later stage this might also do health
			/// and then the defender would be the same as the attacker. For now, though, it is always the enemy.
			/// </summary>
			defender;

		/// <summary>
		/// The prefab to instantiate to display the damage on screen.
		/// There is a separate camera attached to the scene that only renders the <c>TextOverlays</c> layer.
		/// Make sure your prefab and all it's children is set to that layer to make sure it renders on top of everything.
		/// </summary>
		public tbbDamageText
			damage_prefab;

		/// <summary>
		/// Indicates where the damage indicators will be spawned. Will it spawn on the enemy or the player?
		/// </summary>
		public tbbeSpawnOrigin
			damage_spawn_location = tbbeSpawnOrigin.Source;

		/// <summary>
		/// Indicates wether all blows have been dealt
		/// </summary>
		[System.NonSerialized]
		public bool 
			done_attacking = false;

		/// <summary>
		/// If the damage displays immediately then the damage will display on screen before the character
		/// was able to play the attack animation. Here you get to put a delay before the damage is dealt.
		/// Additionally, since some attacks can deal multiple blows, here you can specify a delay between each attack
		/// which will manifest itself visually as a delayed damage spawn.
		/// </summary>
		[SerializeField]
		public _delayed_damage[]
			delayed_damage;

		/// <summary>
		/// Every single game will have it's own calculations to deal damage as such I created this as a virtual function
		/// so you can override it with your own damage calculation function. This version of the function calls a coroutine
		/// that deal damage over time so wether it is a single punch or an attack that punches 10 times, both are covered
		/// by simply stating the delay between damages dealt and then how much damage to deal at that time.
		/// </summary>
		/// <param name="level">Level.</param>
		virtual public void DealDamage(int level)
		{
			StartCoroutine(DoDelayedDamage(level));
		}

		/// <summary>
		/// For each entry in delayed_damage this will wait the specified amount of time then deal damage. Again, this is
		/// merely my version of a damage calculation routine and you are encouraged to write your own. For this version
		/// I simply take the player's level and multiply the attack's strength by that amount. I then trigger an event in
		/// case you ARE using this function of mine but want to have some extra control over the damage calculation.
		/// The event will pass along the amount of damage dealt for that blow as it's only parameter.
		/// 
		/// Finally, if the defender is to die from this attack, this will pass along the message to the character's
		/// tbbPlayerInfo object who, in turn, should have been configured with an appropriate dying action from the
		/// tbbFaction to respond to the death of one of it's characters.
		/// </summary>
		/// <param name="level">The level of the currently attacking character</param>
		virtual public IEnumerator DoDelayedDamage(int level)
		{
			done_attacking = false;
			int index = 0;
			do
			{
				yield return new WaitForSeconds(delayed_damage[index].delay);
				int amount = delayed_damage[index].damage * level;
				defender.HP -= amount;
				if (null != onDamageDealt)
					onDamageDealt(amount);

				if (null != defender.onDamageTaken)
					defender.onDamageTaken(amount);

			} while (++index < delayed_damage.Length);

			done_attacking = true;
			if (null != onAllDamageDealt)
				onAllDamageDealt();

			if (defender.HP <= 0)
				if (null == defender.onDied)
					Debug.LogError("Character is not configured to die: " + defender.character_name);
				else
					defender.onDied(defender);
		}

		/// <summary>
		/// You are meant to define your own damage handling routines but in case you are happy with my system for dealing
		/// damage over time, I trigger the onDamageDealt event each time a blow deals damage and this function is one of
		/// the responders I have setup for that event. 
		/// 
		/// This function will spawn the damage prefab on layer TextOverlays to display the damage on top of all other objects
		/// in the scene. This function respects the attack's setting for spawn location and will either spawn on the attacker
		/// or on the defender, as specified, but will spawn with a slight offset so as to make the various damage texts
		/// visible and not overlapping each other. To that same end I also vary the speed at which the damage rise up and
		/// the speed at which they vanish.
		/// 
		/// The font is a pure white font with a pure black frame. This means you could also add a color tint to the font and
		/// it will still show up with a solid black frame. In this version of the function I do not tint the color though.
		/// </summary>
		/// <param name="amount">The amount of damage that will be displayed</param>
		virtual public void HandleDealtDamage(int amount)
		{
			Vector3 random_offset = new Vector3(Random.Range(-0.3f,0.3f),Random.Range(0.2f,0.35f),Random.Range(-0.2f,0.2f));
			Vector3 spawn_location = random_offset + (damage_spawn_location == tbbeSpawnOrigin.Destination ? attacker.transform.position : defender.transform.position);
			tbbDamageText damage = (tbbDamageText)Instantiate(damage_prefab, spawn_location, Quaternion.identity);
			damage.rise_speed = Random.Range(0.3f, 0.5f);
			damage.fade_speed = Random.Range(0.3f, 0.45f);
			damage.value = amount;
		}
		
		void Start () {
			onDamageDealt += HandleDealtDamage;
		}
	}
}
