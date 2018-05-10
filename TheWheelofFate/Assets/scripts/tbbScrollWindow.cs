using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	public class tbbScrollWindow : MonoBehaviour {

		public enum tbbeScrWinStatus		{ Hidden, Visible, Focussed }
		public enum tbbeScrWinScaleMode		{ Up, Down }

		public System.Action<int>
			response;

		public mbsStateMachine<tbbeScrWinStatus>
			state;

		mbsStateMachineLeech<tbbeScrWinStatus>
			update_state;

		public mbsSlider
			area;

		public tbbeScrWinScaleMode
			stretch_direction;

		public Texture2D
			cursor;

		public int
			rows_to_show = 5,
			cols = 1,
			selected = 0;

		public float 
			element_height	= 24f,
			element_width	= 150f,
			margin_h = 5f,
			margin_v = 5f;

		public Rect
			content_margins = new Rect(5,5,5,5);	//spacing between window edges and content

		public GUISkin
			the_skin;

		public bool Focussed 
		{
			get 
			{
				return state.CompareState(tbbeScrWinStatus.Focussed);
			}
			set 
			{
				state.SetState( value ? tbbeScrWinStatus.Focussed : tbbeScrWinStatus.Visible );
			}
		}
		
		public bool Visible
		{
			get 
			{
				return !state.CompareState(tbbeScrWinStatus.Hidden);
			}
			set 
			{
				state.SetState( value ? tbbeScrWinStatus.Visible : tbbeScrWinStatus.Hidden );
			}
		}
		
		public List<object>
			items;

		// Use this for initialization
		void Start () {
			items = new List<object>();
			items.Add("hallow");
			items.Add("world");
			items.Add("piet my vrou");

			state = new mbsStateMachine<tbbeScrWinStatus>();
			state.AddState(tbbeScrWinStatus.Hidden);
			state.AddState (tbbeScrWinStatus.Visible,	ShowWindow);
			state.AddState (tbbeScrWinStatus.Focussed,	ShowFocussed);

			update_state = new mbsStateMachineLeech<tbbeScrWinStatus>(state);
			update_state.AddState(tbbeScrWinStatus.Hidden);
			update_state.AddState (tbbeScrWinStatus.Visible);
			update_state.AddState (tbbeScrWinStatus.Focussed,	UpdateCursor);

			Focussed = true;

			Rect 
				temp = area.targetPos;

			temp.height = content_margins.y+content_margins.height + (rows_to_show * (element_height + margin_v)) - margin_v;
			temp.width = content_margins.x + content_margins.width + (cols * (element_width + margin_h)) - margin_h + 20f;

			if (stretch_direction == tbbeScrWinScaleMode.Up)
				temp.y = area.targetPos.yMax - temp.height;

			area.targetPos = temp;

			area.Init();
			area.ForceState(eSlideState.Closed);

			area.Activate();
		}
	
		// Update is called once per frame
		void Update () {
			area.Update();
			update_state.PerformAction();
		}

		void OnGUI()
		{
			GUI.skin = the_skin;
			state.PerformAction();
		}

		Vector2 scrollposition;

		void ShowWindow(bool focussed)
		{
			GUI.Box (area.Pos,"");

			if (null == items || items.Count == 0)
				return;

			//if only one colomn, create one row per item
			int rows = items.Count;
			if (cols > 1)
			{
				//if more than one colomn, first remove the excess to make sure we can get an equal divide
				//then see how many rows the available items occupy
				//and add an extra row if the items / cols was not an equal divide
				int remainder = items.Count % cols;
				int extra_row = (remainder > 0) ? 1 : 0;
				rows = ((rows - remainder) / cols) + extra_row;
			}

			Rect 
				display_area = new Rect(area.Pos.x + content_margins.x,
			                            area.Pos.y + content_margins.y,
			                            area.Pos.width - (2 * content_margins.width),
			                            area.Pos.height - (2 * content_margins.height)),
				scroll_area = new Rect(0,
				                       0,
				                       area.Pos.width - content_margins.x - content_margins.width - 20f,
				                       rows * (element_height + margin_v) - margin_v);

				scrollposition = GUI.BeginScrollView(display_area,
				                                     scrollposition,
				                                     scroll_area);
			float 
				x = 0,
				y = 0;

			int index = 0;
			int h = 0;
			foreach(object o in items)
			{
				GUI.BeginGroup(new Rect(x,y,element_width, element_height));
				DrawItem (o, index++);
				GUI.EndGroup();
				if (h == cols - 1)
				{
					h = 0;
					x = 0;
					y += element_height + margin_v;
				} else
				{
					h++;
					x += element_width + margin_h;
				}
			}
			if (focussed && null != cursor)
			{

			}
			GUI.EndScrollView();
		}

		virtual public void DrawItem(object item, int index)
		{
			string str_item = (string)item;
			if (GUI.Button(new Rect(0,0,element_width, element_height), str_item))
			{
				if (selected != index)
					index = selected;
				else
					if (null != response)
						response(index);
			}
		}

		void ShowWindow()
		{
			ShowWindow (false);
		}

		void ShowFocussed()
		{
			ShowWindow (true);
		}

		void UpdateCursor()
		{

		}
	}
}
