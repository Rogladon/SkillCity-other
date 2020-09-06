using System.Collections.Generic;
using UnityEngine;

namespace SkillCity {
	[DisallowMultipleComponent]
	public abstract class StateMachine : MonoBehaviour {
		private string objectNameBase;

		public State currentState { get; private set; }

		private List<StateOverseer> _overseers = new List<StateOverseer>();
		public IEnumerable<StateOverseer> overseers { get { return _overseers; } }

		public delegate void OnStateChangeDelegate(State crr, State old);
		public OnStateChangeDelegate onStateChange;

		void Start() {
			OnStateChangeDelegate oscd = (State crr, State old) => {
				foreach (StateOverseer so in overseers) {
					so.OnStateChange(crr, old);
				}
			};
			if (onStateChange == null) {
				onStateChange = oscd;
			} else {
				onStateChange += oscd;
			}
			objectNameBase = gameObject.name;

			Configure();

			if (currentState == null) {
				Debug.LogWarning("After Configure() there is still no currentState");
			}
		}

		void Update() {
			foreach (StateOverseer so in overseers) {
				if (isEnabledFor(so, currentState)) {
					so.PreTick();
				}
			}
			if (currentState != null) {
				currentState.Tick();
			}
			foreach (StateOverseer so in overseers) {
				if (isEnabledFor(so, currentState)) {
					so.PostTick();
				}
			}
		}

		void FixedUpdate() {
			foreach (StateOverseer so in overseers) {
				if (isEnabledFor(so, currentState)) {
					so.FixedPreTick();
				}
			}
			if (currentState != null) {
				currentState.FixedTick();
			}
			foreach (StateOverseer so in overseers) {
				if (isEnabledFor(so, currentState)) {
					so.FixedPostTick();
				}
			}
		}

		public bool isEnabledFor(StateOverseer so, State state) {
			var attrs = so.GetType().GetCustomAttributes(typeof(OffOnState), true);
			if (attrs != null) {
				foreach (OffOnState a in (OffOnState[])attrs) {
					if (currentState == null) {
						if (a.type == null) {
							return false;
						}
					} else if (a.type != null) {
						if (a.type.IsAssignableFrom(currentState.GetType())) {
							return false;
						}
					}
				}
			}
			return true;
		}

		/// <param name="changeEvenIfSame"> Будут ли вызываться обработчики смены состояния даже если <c>state == currentState</c></param>
		public void ChangeState(State state, bool changeEvenIfSame=false) {
			if (state == currentState && !changeEvenIfSame) {
				return;
			}
			if (currentState != null) {
				currentState.OnStateExit();
			}

			var oldState = currentState;

			currentState = state;
			gameObject.name = $"{objectNameBase}~~{(currentState == null ? "null" : currentState.GetType().Name)}";

			if (state != null) {
				state.IntroduceStateMachine(this);
				state.OnStateEnter();
			}
			onStateChange(currentState, oldState);
		}

		public void AddOverseer(StateOverseer so) {
			if (so == null) {
				throw new System.NullReferenceException("so == null");
			}
			_overseers.Add(so);
			so.IntroduceStateMachine(this);
			so.Initialize();
		}

		public void RemoveOverseer(StateOverseer so) {
			_overseers.Remove(so);
		}

		public abstract void Configure();

		public bool assertState(System.Type t) {
			if (currentState == null) {
				return t == null;
			} else {
				return currentState.GetType() == t;
			}
		}

		public bool assertState(State state) {
			return currentState == state;
		}

		private void OnCollisionEnter2D(Collision2D collision) {
			if (currentState != null) currentState.OnCollisionEnter2D(collision);
		}

		private void OnCollisionStay2D(Collision2D collision) {
			if (currentState != null) currentState.OnCollisionStay2D(collision);
		}
		private void OnCollisionExit2D(Collision2D collision) {
			if (currentState != null) currentState.OnCollisionExit2D(collision);
		}
	}
}
