using UnityEngine;
using System.Collections;

namespace MBS {
	
	public enum tbbeAttackMode		: byte {InPlace, AtTarget, NeutralArea}
	public enum tbbeControlMethod	: byte {AI, Human}
	public enum tbbeAttackModeExt	: byte {InPlace, AtTarget, NeutralArea, Default}
	public enum tbbAttackPhase		: byte {Idle, SelectTarget, PreOpeningCinematic, OpeningCinematic, OpeningCinematicPlaying, MoveToAttackPosition, Attack, Attacking, ReturnToStand}
	public enum tbbeAttackState		: byte {Config, Ready, Fighting, Done, Dead}
	public enum tbbeBattlefieldSide	: byte {Lower, Upper}
	public enum tbbeBattleState		: byte {Intro1, Switching, Intro2, Player, AI, Battling, ResultsScreen, Victory, Defeat, Retry, Count}
	public enum tbbeBattleType 		: byte {PerFaction, Immediate}//, Realtime}
	public enum tbbeCharSelModes	: byte {CharacterSelect, NewLocationSelect, ActionSelect, ActionRefinementSelect, SpellSelect, ItemSelect, TargetSelect, Count}
	public enum tbbGridModes		: byte {Tabular, Hex}
	public enum tbbeIntroStates		: byte {Waiting, Starting, Displaying, Closing, Done}
	public enum tbbeSpawnOrigin		: byte {Source, Destination}
	public enum tbbETileMode		: byte {Hidden, Normal, Highlighted, ValidSelection, InvalidSelection}
	public enum tbbeActionType		: byte {Attack, Magic, Count}
	
	/* Stuff I want to use in future versions of the kit...
 * -----------------------------------------------------
	public enum tbbeAttackRange		: byte {Close, Medium, Ranged}
	public enum tbbeAlignments		: byte {Earth, Fire, Water, Wind, Metal, Ethereal, Nether, Swamp, Plains}
	public enum tbbeSkillsets		: byte {Haste, Flying, Trample, Unblockable, Swimming, Climbing}
	public enum tbbeModifiers		: byte {CantBlock, CantAttack, Frozen, Poisoned, Burning, SlowedDown, Vertigo, Traitor}
	public enum tbbeWeaponType		: byte {OneHandedFighter, TwoHandedFighter, OneHandedStaff, TwoHandedStaff, OneHandedRanged, TwoHandedRanged, OneHandedMagic, TwoHandedMagic}

	public enum tbbeActionType		: byte {Attack, Magic, Item, Protect, Flee, Count}
*/
	
}
