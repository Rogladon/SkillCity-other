using System.Collections.Generic;
using UnityEngine;

namespace SkillCity {
	public abstract class StateOverseer {
		protected GameObject gameObject { get; private set; }
		protected Transform transform { get { return gameObject.transform; } }

		public virtual void IntroduceStateMachine(StateMachine sm) {
			gameObject = sm.gameObject;
		}
		public virtual void Initialize() {}
		public virtual void PreTick() {}
		public virtual void PostTick() {}

		public virtual void FixedPreTick() {}
		public virtual void FixedPostTick() {}
		public virtual void OnStateChange(State crr, State old) {}
	}

	public abstract class MachineSpecificStateOverseer<T> : StateOverseer where T : StateMachine {
		protected T mch { get; private set; }
		public override sealed void IntroduceStateMachine(StateMachine c) {
			base.IntroduceStateMachine(c);
			mch = (T)c;
		}
	}
}
