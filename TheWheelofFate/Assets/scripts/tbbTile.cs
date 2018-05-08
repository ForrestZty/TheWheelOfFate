using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS{

	/// <summary>
	/// An instance of a tile in the grid
	/// </summary>
	public class tbbTile : MonoBehaviour {

		/// <summary>
		/// In case the tile has multiple renderers, which one should we tint? For instance, the tile could be a model of a hill and you have a plane on tp that you want to tint when selected, insteadof the actual mountain itself.
		/// </summary>
		public Renderer
			tile_renderer;

		/// <summary>
		/// The colors the tiles will be tinted to based on the tile mode enum.
		/// Values enumerate to: Hidden, Normal, Highlighted, ValidSelection, InvalidSelection
		/// </summary>
		public Color[]
			tints = new Color[5]{ new Color(1,1,1,0), Color.white, Color.blue, Color.green, Color.red };

		/// <summary>
		/// Holds a reference to what character is currently standing on it
		/// </summary>
		[System.NonSerialized]
		public tbbPlayerInfo 
			Character;

		tbbETileMode
			tile_mode = tbbETileMode.Normal;

		/// <summary>
		/// Indicate wether this tile is part of the top or bottom half of the battlefield
		/// </summary>
		[System.NonSerialized]
		public tbbeBattlefieldSide
			side = tbbeBattlefieldSide.Upper;

		// Use this for initialization
		void Start () {
			if (null == tile_renderer)
				tile_renderer = transform.GetComponentsInChildren<Renderer>()[0];
		}
	
		/// <summary>
		/// Returns the current tile mode enum and updates the tile's tinting when you set the enum value.
		/// </summary>
		/// <value>The tile mode.</value>
		public tbbETileMode TileMode
		{
			get 
			{
				return tile_mode;
			}

			set 
			{
				tile_mode = value;
				tile_renderer.material.color = tints[ (int)tile_mode ];
			}
		}
	}
}
