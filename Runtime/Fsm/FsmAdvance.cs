using TEngine;

namespace GameLogic {
	public class FsmStateAdvance<T> : FsmState<T> where T : class {
		protected T m_owner;
		protected IFsm<T> m_fsm;

		public void Init(IFsm<T> fsm, T owner) {
			m_fsm = fsm;
			m_owner = owner;
			OnInit();
		}

		/// <summary>
		/// 延迟初始化(所有状态已经创建完毕）
		/// </summary>
		protected virtual void OnInit() {
		}

		/// <summary>
		/// 切换当前有限状态机状态。
		/// </summary>
		/// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
		protected new void ChangeState<TState>() where TState : FsmState<T> {
			if (m_fsm.CurrentState is TState) return;
			base.ChangeState<TState>(m_fsm);
		}
	}

	public static class FsmExtensions {
		/// <summary>
		/// 创建高级有限状态机
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>要创建的有限状态机。</returns>
		public static IFsm<T> CreateFsmAdvanced<T>(this IFsmModule fsmModule, T owner, params FsmStateAdvance<T>[] states) where T : class {
			var fsm = fsmModule.CreateFsm(string.Empty, owner, states);
			foreach (var fsmStateAdvance in states) {
				fsmStateAdvance.Init(fsm, owner);
			}

			return fsm;
		}
	}
}