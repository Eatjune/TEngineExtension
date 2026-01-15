using System;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameLogic {
	public class SceneCom_LoadAsyncScene : MonoBehaviour {
		//显示进度的文本
		public Text progress;

		//进度条
		public Transform Slider;

		//进度条的数值
		private float progressValue;

		/// <summary>
		/// 下个场景的名字
		/// </summary>
		public string NextSceneName;

		private InputSystem_Actions m_inputSystemActions;

		/// <summary>
		/// 最少读取秒
		/// </summary>
		public static float MinLoadingTime = 0.3f;

		private float m_displayProgress;
		private float m_timer;
		private bool m_suspendLoad;

		private bool m_loadFinish;

		public UniTask<Scene> LoadScene(bool suspendLoad, LoadSceneMode mode = LoadSceneMode.Additive) {
			m_displayProgress = m_timer = 0f;
			m_suspendLoad = suspendLoad;
			m_loadFinish = false;
			GameEvent.Get<IEventScene>().LoadScene(NextSceneName);
			return GameModule.Scene.LoadSceneAsync(NextSceneName, mode, true, 100, true, OnLoadingSceneProgress);
		}

		private void OnLoadingSceneProgress(float _progress) {
			if (m_loadFinish) {
				return;
			}

			var targetProgress = _progress >= 0.9f ? 1f : _progress;

			// 增加时间累计
			m_timer += Time.unscaledDeltaTime;

			// 计算插值目标：在最小加载时间内匀速过渡到 1
			var maxProgressAllowed = Mathf.Min(m_timer / MinLoadingTime, targetProgress);
			m_displayProgress = Mathf.MoveTowards(m_displayProgress, maxProgressAllowed, Time.unscaledDeltaTime * (1 / MinLoadingTime));

			Slider.localScale = new Vector3(m_displayProgress, 1, 1);
			progress.text = (int)(m_displayProgress * 100) + " %";

			// 如果满足两个条件：时间到、进度到
			if (m_timer >= MinLoadingTime && m_displayProgress >= 1f) {
				if (m_suspendLoad) {
					progress.text = "按任意键继续";
					foreach (var key in Keyboard.current.allKeys) {
						if (key is {wasPressedThisFrame: true}) {
							if (GameModule.Scene.UnSuspend(NextSceneName)) {
								// GameModule.Scene.ActivateScene(NextSceneName);
								GameModule.Scene.UnloadAsync("Loading");
								Log.Debug($"Open scene : {NextSceneName}");
								GameEvent.Get<IEventScene>().OpenScene(NextSceneName);
								m_loadFinish = true;
								break;
							}
						}
					}
				} else {
					if (GameModule.Scene.UnSuspend(NextSceneName)) {
						GameModule.Scene.UnloadAsync("Loading");
						Log.Debug($"Open scene : {NextSceneName}");
						GameEvent.Get<IEventScene>().OpenScene(NextSceneName);
						m_loadFinish = true;
					}
				}
			}
		}
	}
}
