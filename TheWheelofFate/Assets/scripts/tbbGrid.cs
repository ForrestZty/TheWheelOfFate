using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// This class contains the functions related to creating the actual grid as well as the highlighting of the tiles
	/// This class also checks to see if tiles are available for occupation, sets and clears the ocupation
	/// </summary>
	public class tbbGrid : ScriptableObject {
		
		[System.NonSerialized]
		public GameObject 
			camp;
		
		int cols 		{ get { return tbbBattleField.Instance.grid_width; } } 
		int rows 		{ get { return tbbBattleField.Instance.grid_height; } set { tbbBattleField.Instance.grid_height = value; } } 
		float tile_size { get { return tbbBattleField.Instance.tile_size; } } 
		float margin	{ get { return tbbBattleField.Instance.tile_margin; } } 
		
		[System.NonSerialized]
		public tbbTile[,]
		tiles;
		
		
		/// <summary>
		/// A marker to identify the top edge of the battlefield
		/// </summary>
		public Transform	North;
		
		/// <summary>
		/// A marker to identify the lower edge of the battlefield
		/// </summary>
		public Transform	South;
		/// <summary>
		/// A marker to identify the right edge of the battlefield
		/// </summary>
		public Transform	East;
		
		/// <summary>
		/// A marker to identify the left edge of the battlefield
		/// </summary>
		public Transform	West;
		
		/// <summary>
		/// A marker to identify the middle of the battlefield
		/// </summary>
		public Transform	Middle;
		
		/// <summary>
		/// Creates the field and makes sure the relevant objects exist and are parented where it belongs
		/// </summary>
		/// <param name="name">A name to assign to this gameobject</param>
		/// <param name="parent">A reference to the object that is to contain this battlefield</param>
		public void PrepareField(string name, Transform parent)
		{	
			camp = new GameObject(name);
			CreateGrid(parent);
			camp.transform.parent = parent;
		}
		
		/// <summary>
		/// Create and position the tiles that form the playing field for this faction. Force the row count to be an equal number
		/// </summary>
		public void CreateGrid(Transform parent){CreateGrid(parent, true);}
		/// <summary>
		/// Create and position the tiles that form the playing field for this faction. Optionally force the row count to be an equal number
		/// </summary>
		public void CreateGrid(Transform parent, bool force_equal)
		{
			if (force_equal && rows % 2 > 0)
				rows++;
			
			bool
				odd = false;
			
			int
				grid_break = rows / 2;
			
			float
				half_tile = tile_size / 2f,
				center_h = ((cols * (margin + tile_size))- margin) / 2f,
				total_height = (rows * (tile_size * (tbbBattleField.Instance.grid_style == tbbGridModes.Hex ? 0.75f : 1f)))
					+ ((rows - 1)*margin)
					+ tbbBattleField.Instance.neutral_grounds,
					center_v = total_height/2f;
			
			tiles = new tbbTile[cols, rows];
			for ( int y = 0; y < rows; y++)
			{
				for ( int x = 0; x < cols; x++)
				{	
					tbbTile tile = (tbbTile)Instantiate(tbbBattleField.Instance.tile_prefab);
					tile.transform.parent = camp.transform;
					tile.transform.localRotation = Quaternion.identity;
					tile.transform.localPosition = new Vector3((x * (margin + tile_size)) - center_h + half_tile, 0.01f, (-y * (margin + tile_size)) + center_v - half_tile);
					tile.transform.localScale = new Vector3(tile_size, 1 , tile_size);
					tile.transform.name = "Tile_"+x+"_"+y;
					tiles[x,y] = tile;
					
					switch (tbbBattleField.Instance.grid_style)
					{
					case tbbGridModes.Hex:
						if (odd)
							tile.transform.Translate((margin+tile_size)/ 2f, 0, 0);
						tile.transform.Translate(0,0, y * (tile_size * 0.25f));
						break;
					}
					
					if (y >= grid_break)
					{
						tile.side = tbbeBattlefieldSide.Lower;
						tile.transform.Translate(0,0,-tbbBattleField.Instance.neutral_grounds);
					}
				}
				odd = !odd;
			}
			
			float height = Vector3.Distance( tiles[0,0].transform.position, tiles[ 0, tiles.GetUpperBound(1)].transform.position) + tile_size;
			
			GameObject n = new GameObject("North"); n.transform.parent = camp.transform;
			GameObject s = new GameObject("South"); s.transform.parent = camp.transform;
			GameObject m = new GameObject("Middle"); m.transform.parent = camp.transform;
			GameObject e = new GameObject("East"); e.transform.parent = camp.transform;
			GameObject w = new GameObject("West"); w.transform.parent = camp.transform;
			n.transform.localPosition = new Vector3(tiles[0,0].transform.position.x + center_h - half_tile,
			                                        0.01f,
			                                        tiles[0,0].transform.position.z + half_tile);
			s.transform.localPosition = new Vector3(n.transform.position.x,
			                                        0.01f,
			                                        n.transform.position.z - height);
			m.transform.localPosition = new Vector3(n.transform.position.x,
			                                        0.01f,
			                                        n.transform.position.z - (height/2f));
			e.transform.localPosition = new Vector3(tiles[0,0].transform.localPosition.x - half_tile, 0.01f, m.transform.localPosition.z);
			w.transform.localPosition = new Vector3(tiles[ tiles.GetUpperBound(0), 0].transform.localPosition.x + tile_size, 0.01f, m.transform.localPosition.z);
			North = n.transform;
			South = s.transform;
			Middle = m.transform;
			
			camp.transform.position = parent.position;
			camp.transform.rotation = parent.rotation;
		}
		
		public bool CheckAvailability(Vector2 location, Vector2 space) { return CheckAvailability(location, space, true, null);}
		/// <summary>
		/// If a character requires 2 cols and 2 rows, for instance, check
		/// the grid to see if enough adjacent tiles are unoccupied
		/// </summary>
		/// <returns><c>true</c>, if enough adjacent space is availabile, <c>false</c> otherwise.</returns>
		/// <param name="location">The first of the tiles to start the testing from</param>
		/// <param name="space">How many tiles are required</param>
		/// <param name="confiled">Should the space be on the same side of the battlefield or can the entire field be tested against?</param>
		/// <param name="self_exception">If a tile contains this object, treat it as null and continue testing</param>
		public bool CheckAvailability(Vector2 location, Vector2 space, bool confined, tbbPlayerInfo self_exception)
		{
			int 
				xmax = (int)location.x + (int)space.x - 1,
				ymax = (int)location.y + (int)space.y - 1;
			
			if (xmax < 0 || xmax >= cols || ymax < 0 || ymax >= rows)
				return false;
			int
				startx = (int)location.x,
				starty = (int)location.y;
			
			for ( int c = 0; c < space.x; c++)
			{
				for ( int r = 0; r < space.y; r++)
				{
					tbbTile tile = tiles[startx + c, starty + r];
					
					if (confined && tiles[startx, starty].side != tile.side) return false;
					if (null == tile.Character)
						continue;
					if (null != self_exception && self_exception.CharacterID == tile.Character.CharacterID)
						continue;
					
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Used during character spawning
		/// Finds a random location in the grid large enough to contain a character that takes up Space
		/// If Space is too large or the selected area is occupied, returns Vector2(1,-1)
		/// </summary>
		/// <returns>The top left tile from the selected region</returns>
		/// <param name="space">The amount of tiles required</param>
		/// <param name="side">Look in top or bottom part of the grid</param>
		public Vector2 FindRandomLocation(Vector2 space, tbbeBattlefieldSide side)
		{			
			if ((int)space.x > cols) return new Vector2(-1,-1);
			if ((int)space.y > rows) return new Vector2(-1,-1);
			
			int
				x = 0,
				y = 0;
			
			if (space.x < cols)
				x = Random.Range((int)0, (int)(cols - (int)space.x + 1));
			
			if (space.y < rows)
				y = side == tbbeBattlefieldSide.Upper ?
					Random.Range((int)0, (int)(rows / 2) - 1) :
					Random.Range((int)(rows / 2), (int)(rows-1));
			
			Vector2 location = new Vector2(x, y);
			if (!CheckAvailability(location, space))
				return new Vector2(-1,-1);
			
			return location;
		}
		
		/// <summary>
		/// Mark the relevant tiles to say they contain a specified character
		/// This way we know not to place other characters on this tile(s)
		/// </summary>
		/// <param name="location">The starting tile index</param>
		/// <param name="space">How many tiles to mark</param>
		/// <param name="occupatant">Occupatant.</param>
		public void SetOccupation(Vector2 location, Vector2 space, tbbPlayerInfo occupatant)
		{
			int 
				xmax = (int)location.x + (int)(space.x - 1),
				ymax = (int)location.y + (int)(space.y - 1);
			
			int
				startx = location.x < xmax ? (int)location.x : xmax,
				starty = location.y < ymax ? (int)location.y : ymax;
			
			for ( int c = 0; c < space.x; c++)
				for ( int r = 0; r < space.y; r++)
					tiles[startx + c,starty + r].Character = occupatant;
		}
		/// <summary>
		/// Clears the tiles by setting it's occupant to NULL. This will make the tile(s) available for other characters to occupy
		/// </summary>
		/// <param name="location">Location.</param>
		/// <param name="space">Space.</param>
		public void ClearOccupation(Vector2 location, Vector2 space)
		{
			SetOccupation(location,space,null);
		}
		
		public Vector3 Center(Vector2 location, Vector2 space)
		{
			if (space.x == 0) space.x = 1;
			if (space.y == 0) space.y = 1;
			if (location.x + space.x > cols
			    ||  location.y + space.y > rows)
				return tiles[(int)location.x, (int)location.y].transform.position;
			
			tbbTile
				tl = tiles[(int)location.x, (int)location.y],
				tr = tiles[(int)location.x + (int)space.x-1, (int)location.y],
				bl = tiles[(int)location.x, (int)location.y + (int)space.y - 1];
			
			float
				offset_h = Vector3.Distance(tl.transform.position, tr.transform.position) / 2f,
				offset_v = Vector3.Distance(tl.transform.position, bl.transform.position) / 2f;
			
			return new Vector3(tl.transform.position.x + offset_h, tl.transform.position.y, tl.transform.position.z - offset_v);
		}
		
		/// <summary>
		/// Find and tint tiles that claim to contain a specified character
		/// </summary>
		/// <param name="character">The selected character</param>
		/// <param name="color_mode">Select the tint color based on the tile mode enum</param>
		public void TintCharacterTiles(tbbPlayerInfo character, tbbETileMode color_mode)
		{
			for (int c = 0; c < cols; c++)
			{
				for(int r = 0; r < rows; r++)	
				{
					if (tiles[c,r].Character == character)
						SetTileMode(new Vector2(c, r), color_mode);
				}
			}
		}
		
		/// <summary>
		/// Sets all tiles to the normal tint color
		/// </summary>
		public void ResetTileModes()
		{
			for (int c = 0; c < cols; c++)
				for(int r = 0; r < rows; r++)
					SetTileMode(new Vector2(c, r), tbbETileMode.Normal);
		}
		
		/// <summary>
		/// Set a specific tile to a specific tint value
		/// </summary>
		/// <param name="tile">The tile to tint</param>
		/// <param name="tile_mode">Select the tint color based on the tile mode enum</param>
		public void SetTileMode(Vector2 tile, tbbETileMode tile_mode )
		{
			tiles[ (int) tile.x, (int) tile.y].TileMode = tile_mode;
		}
		
		public void SetTileMode(List<tbbTile> tiles, tbbETileMode tile_mode )
		{
			foreach (tbbTile t in tiles) t.TileMode = tile_mode;
		}
	}
}