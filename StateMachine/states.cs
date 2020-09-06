using System.Collections.Generic;
using UnityEngine;

namespace SkillCity {
	public abstract class State {
		protected GameObject gameObject { get; private set; }
		protected Transform transform { get { return gameObject.transform; } }

		public virtual void Tick() {}
		public virtual void FixedTick() {}
		public virtual void IntroduceStateMachine(StateMachine sm) {
			gameObject = sm.gameObject;
		}
		public virtual void OnStateEnter() {}
		public virtual void OnStateExit() {}

		public virtual void OnCollisionEnter2D(Collision2D collision) { }
		public virtual void OnCollisionStay2D(Collision2D collision) { }
		public virtual void OnCollisionExit2D(Collision2D collision) { }
	}

	public abstract class IdleState : State {
		public override sealed void Tick() {}
	}

	public abstract class MachineSpecificState<T> : State where T : StateMachine {
		protected T mch { get; private set; }
		public override sealed void IntroduceStateMachine(StateMachine c) {
			base.IntroduceStateMachine(c);
			mch = (T)c;
		}
	}

	public abstract class MachineSpecificIdleState<T> : MachineSpecificState<T> where T : StateMachine {
		public override sealed void Tick() {}
	}
}
