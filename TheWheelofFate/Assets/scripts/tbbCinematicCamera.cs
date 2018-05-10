using UnityEngine;
using System.Collections;

namespace MBS{
	public class tbbCinematicCamera : MonoBehaviour {

		public string 
			animation_name;

		public AudioClip
			audio_clip;

		public System.Action onCamDone;
		
		Animation 
			anim;
		
		AnimationClip
			anim_clip;
		
		// Use this for initialization
		void Start () {
			if (animation_name.Trim() == string.Empty)
			{
				Destroy ( gameObject );
				return;
			}
			
			anim = GetComponentInChildren<Animation>();
			if (null == anim)
			{
				Destroy(gameObject);
			} else
			{
				anim_clip = anim.GetClip(animation_name);
				if (null == anim_clip)
				{
					Destroy(gameObject);
				} else
				{
					anim.clip = anim_clip;
					anim.Play();

					AudioSource Audio = GetComponentInChildren<AudioSource>();
					if (Audio && audio_clip)
					{
						Audio.clip = audio_clip;
						Audio.Play();
					}
					Invoke ("OnCameraDone", anim.clip.length);
				}
			}
			
		}
		
		void OnCameraDone()
		{
			if (null != onCamDone)
				onCamDone();
			Destroy(gameObject);
		}
	}
}
