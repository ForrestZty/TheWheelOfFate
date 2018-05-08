using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MBS {
	public class mbsStateMachineLeech<T> {

		mbsStateMachine<T> source;
		public mbsStateMachine<T> Source { get { return source; } }

		T currentState { get { return source.CurrentState; } }
		Action currentAction;

		public Dictionary<T, Action>			StateFunctions;
	
		public mbsStateMachineLeech(mbsStateMachine<T> _source) {
			this.source = _source;
			StateFunctions = new Dictionary<T, Action>();
		}
		
		public T CurrentState	{ get {	return currentState;	} }
		
		public bool PerformAction() {
			if (StateFunctions.ContainsKey(currentState) && null != StateFunctions[currentState]) 
				StateFunctions[currentState]();

			return true;
		}

		public bool CompareState(T state) {
			return source.CompareState(state);
		}

		public void AddState(T statename, Action function = null) {
			if (null == statename)
				return;
			
			if (StateFunctions.ContainsKey(statename)) 
				StateFunctions[ statename ] = function;
			else
				StateFunctions.Add(statename, function);
		}
	
		public bool RemoveState(T statename) {
			if (StateFunctions.ContainsKey(statename)) {
				StateFunctions.Remove( statename );
				return true;
			}
			return false;
		}
	}
}
