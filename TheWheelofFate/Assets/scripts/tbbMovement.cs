using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	/// <summary>
	/// The movement class is in charge of handling the character's navigation across the grid.
	/// </summary>
	public class tbbMovement : MonoBehaviour {
		
		static public tbbMovement ActiveCharacter
		{
			get
			{
				if (!tbbBattleField.active_faction.LastBattleOrderEntry) return null;
				return tbbBattleField.active_faction.LastBattleOrderEntry.character.GetComponent<tbbMovement>();
			}
		}
		tbbBattleField	battlefield { get { return tbbBattleField.Instance; } }
		
		/// <summary>
		/// These are the valid tiles a character can move to
		/// </summary>
		[System.NonSerialized] public List<tbbTile>	available_tiles;
		
		/// <summary>
		/// These are the tiles within the character's range but that can't be moved to either because it will exceed
		/// the bounds of the grid or because it will overlap with an occupied tile
		/// </summary>
		[System.NonSerialized] public List<tbbTile>	invalid_tiles;
		
		public int
			max_move_range = 2;
		
		/// <summary>
		/// If this turn is entirely cancelled, return to where the character was before
		/// </summary>
		Vector2 	previous_location = new Vector2(-1,-1);
		/// <summary>
		/// The proposed location to move this character to this turn
		/// </summary>
		Vector2 	proposed_location;
		
		Vector3 
			previous_position = new Vector3(-1,-1,-1);
		
		Quaternion
			previous_rotation;
		
		Transform
			previous_parent = null;
		
		static Transform
			cursor;
		
		tbbPlayerInfo
			character;
		
		void Start()
		{
			character = GetComponent<tbbPlayerInfo>();
			if (null == character)
				enabled = false;
		}

		/// <summary>
		/// Fetchs the tiles this character can reach and sorts them into the available_tiles and invalid_tiles lists as appropriate
		/// </summary>
		virtual public void GetAvailableTiles()
		{
			int
				minx = (int)previous_location.x - max_move_range,
				miny = (int)previous_location.y - max_move_range,
				maxx = (int)previous_location.x + max_move_range,
				maxy = (int)previous_location.y + max_move_range;
			if (minx < 0) minx = 0;
			if (miny < 0) miny = 0;
			
			available_tiles = new List<tbbTile>();
			invalid_tiles = new List<tbbTile>();
			
			for ( int x = minx; x <= maxx; x++)
				for ( int y = miny; y <= maxy; y++)
			{
				if (x + (int)character.tiles_required.x -1 >= battlefield.grid_width) continue;
				if (y + (int)character.tiles_required.y -1 >= battlefield.grid_height) continue;
				
				tbbTile tile = battlefield.grid.tiles[x,y];
				if (battlefield.confine_movement && tile.side != tbbBattleField.active_faction.battlefield_side) continue;
				
				if (!battlefield.grid.CheckAvailability(new Vector2(x,y), character.tiles_required, battlefield.confine_movement, character))
					invalid_tiles.Add(tile);
				else
					available_tiles.Add(tile);
			}
		}

		/// <summary>
		/// Tints the tiles to indicate it's availability
		/// </summary>
		public void HighlightAvailableTiles()
		{
			battlefield.grid.SetTileMode(available_tiles, tbbETileMode.Highlighted);
			battlefield.grid.SetTileMode(invalid_tiles, tbbETileMode.InvalidSelection);
		}
		
		public void ClearHighlitedTiles() { ClearHighlitedTiles(true); }
		/// <summary>
		/// Clears the highlited tiles.
		/// </summary>
		/// <param name="highlight_character">If set to <c>true</c> highlight the tiles character is on</param>
		public void ClearHighlitedTiles(bool highlight_character)
		{
			battlefield.grid.ResetTileModes();
			if (highlight_character)
				battlefield.grid.TintCharacterTiles(character, tbbETileMode.Highlighted);
		}

		/// <summary>
		/// Stores the characters starting details at the start of the turn.
		/// Make sure to only call it once and only at the very start of the turn or else this will not serve any purpose
		/// </summary>
		public void StoreOrigin()
		{
			proposed_location =
				previous_location = character.tile_index;
			previous_position = character.transform.position;
			previous_rotation = character.transform.rotation;
			previous_parent = character.transform.parent;
		}

		void RestoreToOrigin()
		{
			if (null == previous_parent) return;
			battlefield.grid.ClearOccupation(character.tile_index, character.tiles_required);
			battlefield.grid.SetOccupation(previous_location, character.tiles_required, character);
			battlefield.grid.ResetTileModes();
			
			character.transform.position = previous_position;
			character.transform.rotation = previous_rotation;
			character.transform.parent = previous_parent;
			proposed_location = previous_location;
			previous_parent = null;
		}

		/// <summary>
		/// This function is the equavalent of hitting the Enter button to say "Yes, I am happy with this location. Continue" 
		/// </summary>
		public void LockDownNewPosition()
		{
			if (battlefield.grid.tiles[(int)proposed_location.x, (int)proposed_location.y].TileMode == tbbETileMode.Highlighted)
			{
				battlefield.grid.ClearOccupation(previous_location,character.tiles_required);
				tbbBattleField.active_faction.PositionCharacter(character, proposed_location, character.tiles_required);
				
				tbbBattleOrder bo =	tbbBattleField.active_faction.LastBattleOrderEntry;
				bo.PositionMarkersAtCharacter();
				
				tbbGUICursor.Instance.Display = true;
				tbbCharSelect.Instance.ChangeToNextState();
			}
		}
		
		/// <summary>
		/// Restores to character to the position it was in at the very start of it's turn.
		/// Used when a turn is cancelled. Changes ownerships on the character and the tiles
		/// </summary>
		public void RevertToPreviousState()
		{
			RestoreToOrigin();
			DestroyCursor();
			if (null != tbbGUICursor.Instance)
				tbbGUICursor.Instance.Display = true;
		}

		/// <summary>
		/// Moves the cursor left by updating proposed_location
		/// </summary>
		public void MoveSelectionLeft()
		{
			if (proposed_location.x > 0) proposed_location.x--;
			PositionCursor();
		}
		
		/// <summary>
		/// Moves the cursor right by updating proposed_location
		/// </summary>
		public void MoveSelectionRight()
		{
			if (proposed_location.x < battlefield.grid_width - 1) proposed_location.x++;
			PositionCursor();
		}
		
		/// <summary>
		/// Moves the cursor up by updating proposed_location
		/// </summary>
		public void MoveSelectionUp()
		{
			if (proposed_location.y > 0) proposed_location.y--;
			PositionCursor();
		}
		
		/// <summary>
		/// Moves the cursor down by updating proposed_location
		/// </summary>
		public void MoveSelectionDown()
		{
			if (proposed_location.y < battlefield.grid_height - 1) proposed_location.y++;
			PositionCursor();
		}
		
		/// <summary>
		/// Moves the cursor to the tile indicated by proposed_location
		/// </summary>
		public void PositionCursor()
		{
			if (null != cursor)
				cursor.position = battlefield.grid.tiles[(int)proposed_location.x, (int)proposed_location.y].transform.TransformPoint(0,0.02f,0);
		}

		/// <summary>
		/// Destroies the cursor prefab from the scene
		/// </summary>
		public void DestroyCursor()
		{
			if (null != cursor)
				Destroy(cursor.gameObject);
		}

		/// <summary>
		/// This function responds to the various state changes performed by the system.
		/// Since this will be called on all characters, this function first checks for an exit in case the character ths is on is not the active character
		/// </summary>
		/// <param name="state">State.</param>
		public void OnCharSelectStateChanged(tbbeCharSelModes state)
		{
			//this is called on all characters so first thing's first,
			//make sure this message is meant for this character
			if (!tbbBattleField.active_faction.LastBattleOrderEntry || character != tbbBattleField.active_faction.LastBattleOrderEntry.character)
				return;
			
			switch(state)
			{
			case tbbeCharSelModes.NewLocationSelect:
				//first clear the character's highlights in case it uses more than one tile and it will interfere with
				//showing the tile the character can move to
				tbbBattleField.Instance.grid.TintCharacterTiles(tbbBattleField.active_faction.LastBattleOrderEntry.character, tbbETileMode.ValidSelection);
				
				GetAvailableTiles();
				HighlightAvailableTiles();
				if (battlefield.tile_cursor)
				{
					if (null == cursor)
						cursor = (Transform)Instantiate(battlefield.tile_cursor);
					
					PositionCursor();
				}
				
				//let's remove the character from it's current location on the grid. Both selecting a new tile and canceling this state will restore it
				//to the field so this should be safe...
				battlefield.grid.ClearOccupation(character.tile_index, character.tiles_required);
				
				//hide the hand pointer
				tbbGUICursor.Instance.Display = false;
				break;
				
			case tbbeCharSelModes.ActionSelect:
				ClearHighlitedTiles();
				DestroyCursor();
				break;
			}
		}
		
	}
}
