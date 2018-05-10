using UnityEngine;
using System.Collections;

namespace MBS{
	/// <summary>
	/// This component turns an int value into a visual graphic on screen.
	/// It uses an image 640x64 in size to represent the 10 digits so you can design the text
	/// to look any way you want. If you keep the text white, though, you can also tint the text.
	/// 
	/// This component requires a prefab that contains a single child object. The main game object will
	/// automatically rise while the alpha value of the second object gradually decreases.
	/// Once the second object is completely invisible, the object is destroyed.
	/// 
	/// For the duration of this object's lifespan, the child object will point towards the MainCamera.
	/// 
	/// See also: <c>tbbDamage.cs</c> for an example of how to use this so it always draws on top of
	/// all other objects in yoru scene.
	/// </summary>
	public class tbbDamageText : MonoBehaviour {

		/// <summary>
		/// The child object of this prefab. The generated texture will be placed on this object's material
		/// and this object's size will be scaled horizontally to match the number of digits to display
		/// </summary>
		public Renderer 
			damage_renderer;

		public float 
			/// <summary>
			/// How fast the damage will move after being spawned
			/// </summary>
			rise_speed = 0.15f,
			/// <summary>
			/// How fast the damage will fade from view
			/// </summary>
			fade_speed = 0.15f;

		/// <summary>
		/// The integer amount to turn into a graphical representation
		/// </summary>
		public int
			value = 100;

		/// <summary>
		/// In case you want to tint the damage value into another color, set that color here.
		/// Be sure to set the alpha value. People tend to forget about that and then they see
		/// nothing on screen at all.
		/// </summary>
		public Color 
			text_color = Color.white;

		/// <summary>
		/// You can create your fonts in Photoshop and make them as fancy as you desire.
		/// You can have as many such fonts as you desire. This variable should point to the one you want to use
		/// </summary>
		public Texture2D
			font_texture;

		Texture2D texture;

		/// <summary>
		/// Create a Texture2D that contains the graphical representation on an int value
		/// </summary>
		/// <returns>The generated texture using the font texture specified</returns>
		/// <param name="value">Value.</param>
		/// <param name="font_texture">Font_texture.</param>
		static public Texture2D IntToTex(int value, Texture2D font_texture)
		{
			string vs = value.ToString();
			char[] ca = new char[vs.Length];
			for(int i = 0; i < ca.Length; i++)
				ca[i] = vs[i];
			Texture2D texture = new Texture2D(ca.Length * font_texture.height, font_texture.height);

			int[] ia = new int[ca.Length];
			for(int i = 0; i < ca.Length; i++)
			{
				ia[i] = int.Parse(ca[i]+"");
				Color[] pixels = font_texture.GetPixels(ia[i] * font_texture.height, 0, font_texture.height, font_texture.height);
				texture.SetPixels(i * font_texture.height, 0, font_texture.height, font_texture.height,pixels);
			}
			texture.Apply();
			return texture;
		}

		/// <summary>
		/// This function handles everything from fetching a texture to apply to the specified renderer to tinting and scaling of said renderer
		/// </summary>
		/// <param name="value">Value.</param>
		public void Init(int value)
		{
			if (null == damage_renderer || !font_texture)
				return;
			damage_renderer.material.mainTexture = IntToTex(value, font_texture);
			damage_renderer.material.color = text_color;
			transform.localScale = new Vector3((int)(damage_renderer.material.mainTexture.width / font_texture.height),1,1);
		}

		void Start()
		{
			Init (value);
		}

		// Update is called once per frame
		void Update () {
			transform.Translate(new Vector3(0,Time.deltaTime * rise_speed,0));
			if(damage_renderer)
			{
				Color temp_color = damage_renderer.material.color;
				temp_color.a -= Time.deltaTime * fade_speed;
				if (temp_color.a < 0)
					temp_color.a = 0;
				damage_renderer.material.color = temp_color;
				if (temp_color.a == 0)
					Destroy(gameObject);
				damage_renderer.transform.LookAt(Camera.main.transform);
			}
		}
	}
}
