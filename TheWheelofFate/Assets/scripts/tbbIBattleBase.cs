using UnityEngine;
using System.Collections;

namespace MBS {
	interface tbbIBattleBase {
		void OnBattleOrderTargetSelected (tbbBattleOrder entry); 	//selected the target
		void OnBattleOrderEntryConfigured (tbbBattleOrder entry);	//went through all phases of configuration
		void OnBattleOrderEntryTurnCompleted (tbbBattleOrder entry);	//went through all phases of combat		
	}
}