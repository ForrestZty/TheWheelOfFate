using UnityEngine;
using System;
using System.Collections;

namespace MBS {
	
	[RequireComponent (typeof(AudioSource)) ]
	
	/// <summary>
	/// Contains info relating to the characters on the battlefield.
	/// 
	/// From here you get to specify what animation to play on the selected character when performing basic
	/// functions like idle and run. You also specify the icon to show during battle selections and how much
	/// space this character takes up on the board. It also contains a lot of extra stuff not used by this 
	/// system at the moment but which may be added in a future update. These includes things like elemental
	/// alignment and various modifiers like the ability to fly and so forth.
	/// </summary>
	public class tbbPlayerInfo : MonoBehaviour {
		
		
		/// <summary>
		/// The name of your character
		/// </summary>
		public string	character_name;
		
		/// <summary>
		/// The name of the run animation to play
		/// </summary>
		public string	run_animation_name = "Run";
		
		/// <summary>
		/// TThe name of the run backwards animation to play
		/// </summary>
		public string	run_back_animation_name = "RunBack";
		
		/// <summary>
		/// The name of the dying animation to play
		/// </summary>
		public string	die_animation_name = "Die";
		
		/// <summary>
		/// The name of the animation to play when taking daamage
		/// </summary>
		public string	damage_front_animation = "DamageFront";
		
		/// <summary>
		/// Not currently implmented, here I want to make the character vary their idle animation.
		/// This is where you list the names of the animations to be played
		/// </summary>
		public string[]
		idle_animation_names = new string[]{"Idle"};
		
		/// <summary>
		/// Not used in this version, this will tell the system when a character in real-time mode
		/// is ready to enter battle
		/// </summary>
		public Action<tbbPlayerInfo>
			onTimerRecharged;
		
		/// <summary>
		/// This is triggered by the damage refab when the character's health is depleted
		/// </summary>
		public Action<tbbPlayerInfo> 
			onDied;
		
		/// <summary>
		/// Triggered when a blow lands. Each attack can deliver multiple blows and each one will trigger this callback
		/// </summary>
		public Action<int>
			onDamageTaken;
		
		/// <summary>
		/// audio to play when taking damage
		/// </summary>
		public AudioClip	hurt_audio;
		
		/// <summary>
		/// Audio to play when dying
		/// </summary>
		public AudioClip	die_audio;
		
		/// <summary>
		/// The actual character model to spawn for this character
		/// </summary>
		public Transform 
			model;
		
		/// <summary>
		/// Not implemented in this version, this indicates wether a character in real-time mode is ready to attack or wether his cooldown timer is still in effect
		/// </summary>
		public bool 
			needs_recharge =  false;
		
		/// <summary>
		/// The image you will see during character select screens
		/// </summary>
		public Texture2D
			avatar;
		
		
		/// <summary>
		/// How long should the timer take to fill up
		/// </summary>
		public float	recharge_time = 5f;
		
		/// <summary>
		/// How fast should the timer be updated. Keep at 1 for real time or modify to apply boosts or curses
		/// </summary>
		public float	recharge_speed = 1f; 
		
		//How many tiles does this character require on the battle field?
		public Vector2
			tiles_required = new Vector2(1,1);
		
		
		/// <summary>
		/// Health points. Die when this reahes 0
		/// </summary>
		public int	HP;
		
		/// <summary>
		/// Magic points. Depleted when you cast a spell
		/// </summary>
		public int	MP;
		
		/// <summary>
		/// Max health this player can have
		/// </summary>
		public int	MaxHP;
		
		/// <summary>
		/// Max magic points this player can have
		/// </summary>
		public int	MaxMP;
		
		/// <summary>
		/// The player's level. In the basic damage system included with this kit, damage is multiplied by the player's level when attacking
		/// </summary>
		public int	Level = 1;
		
		/// <summary>
		/// An array of all the melee attacks this character can perform
		/// </summary>
		public tbbAttackInfo[]
		attacks;
		
		/// <summary>
		/// An array of all the magic this character can cast
		/// </summary>
		public tbbSpellInfo[]
		spells;
		
		//Future use...
		//		public tbbeAlignments[]
		//			alignments;
		
		//		public tbbeSkillsets[]
		//			skills;
		
		//		public tbbeModifiers[]
		//			modifiers;
		
		/// <summary>
		/// What action is this character going to perform? Melee, magic, flee, what?
		/// </summary>
		[System.NonSerialized]
		public int 
			selected_action;
		
		/// <summary>
		/// A means by which to identify individual characters on the board when using duplicates
		/// </summary>
		/// <value>The character I.</value>
		public int CharacterID { get { return character_id; }}
		
		/// <summary>
		/// Is this character reato to attack. Used by real-time mode to probe wether the cooldown timer is full
		/// </summary>
		/// <value><c>true</c> if ready to attack; otherwise, <c>false</c>.</value>
		public bool
		ready_for_attack { get { return recharge_timer == recharge_time; } }
		
		/// <summary>
		/// The target of this character's attack
		/// </summary>
		[System.NonSerialized] public tbbPlayerInfo		target;
		
		/// <summary>
		/// The current state of this character's attack. Configured? Waiting? Dead?
		/// </summary>
		[System.NonSerialized] public tbbeAttackState	attack_state; 
		
		/// <summary>
		/// A reference to this character's animator component that plays the relevant animations on the character
		/// </summary>
		[System.NonSerialized] public Animator			controller;
		
		/// <summary>
		/// A reference to this character's legacy animations if present
		/// </summary>
		[System.NonSerialized] public Animation			anim;
		
		/// <summary>
		/// An internal reference to help locate the character in the battlefield array
		/// </summary>
		[System.NonSerialized] public Vector2			tile_index;
		
		static int __character_id = 0;
		int
			character_id;
		
		Transform
			_model;
		
		float 
			recharge_timer;
		
		/// <summary>
		/// Is this character currently facing away from the attacker
		/// </summary>
		[System.NonSerialized]
		public bool 
			facing_away = false;
		
		AudioSource _audio;
		AudioSource Audio
		{
			get 
			{
				if (null == _audio)
				{
					_audio = GetComponent<AudioSource>();
					if (null == _audio)
						_audio = gameObject.AddComponent<AudioSource>();
				}
				return _audio;
			}
		}
		
		void Start () {
			if (null == model)
			{
				Debug.LogError("No model specified for " + transform.name);
				return;
			}
			
			if (tiles_required.x < 1) tiles_required.x = 1f;
			if (tiles_required.y < 1) tiles_required.y = 1f;
			character_id = ++__character_id;
			attack_state = tbbeAttackState.Config;
			
			_model = (Transform)Instantiate(model);
			_model.parent = transform;
			_model.localPosition = Vector3.zero;
			_model.localRotation = Quaternion.identity;
			
			controller = GetComponentInChildren<Animator>();
			anim = GetComponentInChildren<Animation>();
			
			onDamageTaken += PlayDamageAnimation;
			onDied += DoDying;
			
			if (null == controller)
			{
				if (null == anim)
					Debug.LogError("Animation component not found for " + transform.name);
				else anim.Play(idle_animation_names[0]);
			} 
			else
				controller.Play(idle_animation_names[0]);
		}
		
		void PlayDamageAnimation(int amount)
		{
			if (Audio && hurt_audio)
			{
				if ((Audio.clip == hurt_audio && !Audio.isPlaying) || Audio.clip != hurt_audio)
				{
					Audio.clip = hurt_audio;
					Audio.loop = false;
					Audio.PlayOneShot(hurt_audio);
				}
			}
			
			if (null != anim)
			{
				anim.Play(damage_front_animation);
				anim.CrossFadeQueued(idle_animation_names[0]);
			} else
			{
				controller.Play(damage_front_animation);
			}
		}
		
		void PlayDieAnimation()
		{
			if (Audio && die_audio)
			{
				if ((Audio.clip == die_audio && !Audio.isPlaying) || Audio.clip != die_audio)
				{
					Audio.clip = die_audio;
					Audio.loop = false;
					Audio.PlayOneShot(die_audio);
				}
			}
			
			if (null != anim)
				anim.Play(die_animation_name);
			else
				controller.Play(die_animation_name);
		}
		
		void DoDying(tbbPlayerInfo character)
		{
			attack_state = tbbeAttackState.Dead;
			PlayDieAnimation();
		}
		
		// Update is called once per frame
		void Update () {
			UpdateRechargeTimer();
		}
		
		void UpdateRechargeTimer()
		{
			if (needs_recharge)
			{
				if (recharge_timer < recharge_time)
				{
					recharge_timer += Time.deltaTime * recharge_speed;
					if (recharge_timer >= recharge_time)
					{
						recharge_timer = recharge_time;
						if (null != onTimerRecharged)
							onTimerRecharged(this);
					}
				}
			} else
			{
				recharge_timer = recharge_time;
			}
		}
		
		/// <summary>
		/// Make the real-time mode character's cooldown timer active
		/// </summary>
		public void ResetTimer()
		{
			recharge_timer = 0f;
		}
		
		/// <summary>
		/// Returns the length of the current animation so events can be triggered at the end if it
		/// </summary>
		/// <returns>The animation length.</returns>
		public float CurrentAnimationLength()
		{
			if (anim)
			{
				foreach(AnimationState a in anim)
					if (a.enabled)
						return a.length;
			}
			return controller.GetCurrentAnimatorStateInfo(0).length;
		}
		
		/// <summary>
		/// Selects a random attack to perform
		/// </summary>
		/// <returns>The selected attack</returns>
		public tbbAttackInfo RandomAttack()
		{
			int count = attacks.Length;
			if (count == 0)
				return null;
			if (count == 1)
				return attacks[0];
			return attacks[ UnityEngine.Random.Range(0, count) ];
		}
		
	}
}
