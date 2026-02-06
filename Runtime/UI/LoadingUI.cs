using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LymeUtils.Common;
using TEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Ease = DG.Tweening.Ease;

namespace GameLogic {
	[Window(UILayer.System)]
	public class LoadingUI : UIWindow {
		/// <summary>
		/// 加载场景名
		/// </summary>
		public string LoadSceneName { get; private set; }

		private LoadingUICom m_loadingUICom;
		private InputSystem_Actions m_inputSystemActions;

		private float progressValue;
		private float m_displayProgress;
		private float m_timer;

		private bool m_suspendLoad;
		private bool m_gcCollect;
		private LoadSceneMode m_loadingMode;

		private float m_minLoadingTime;
		private float m_BGFadeInTime = 0f;
		private float m_BGFadeOutTime = 0f;
		private bool m_showProgressBar;
		private bool m_showBackGround;
		private bool m_loadFinish;

		/// <summary>
		/// 最少读取秒
		/// </summary>
		public static float MIN_LOADING_TIME = 0.3f;

		#region 脚本工具生成的代码

		private Image m_imgBackground;
		private GameObject m_goProgressBar;
		private Image m_imgBarBg;
		private Image m_imgBarFg;
		private Text m_textProgress;

		protected override void ScriptGenerator() {
			m_imgBackground = FindChildComponent<Image>("m_imgBackground");
			m_goProgressBar = FindChild("m_goProgressBar").gameObject;
			m_imgBarBg = FindChildComponent<Image>("m_goProgressBar/m_imgBarBg");
			m_imgBarFg = FindChildComponent<Image>("m_goProgressBar/m_imgBarFg");
			m_textProgress = FindChildComponent<Text>("m_goProgressBar/m_textProgress");
		}

		#endregion

		public async UniTask<Scene> LoadScene() {
			//loading背景透明度为1时表示已经准备好
			await UniTask.WaitUntil(() => Math.Abs(m_imgBackground.color.a - 1f) < 0.02f);
			m_goProgressBar.SetActive(m_showProgressBar);
			GameEvent.Get<IEventScene>().LoadScene(LoadSceneName);

			var scene = await GameModule.Scene.LoadSceneAsync(LoadSceneName, m_loadingMode, true, 100, m_gcCollect, OnLoadingSceneProgress);
			return scene;
		}

		private async UniTask OnEnterLoading() {
			//冻结游戏时间
			Time.timeScale = 0f;
			//淡入动画
			var fadeInTime = 0f;
			if (m_loadingUICom is {CurrentParam: { }}) {
				fadeInTime = m_loadingUICom.CurrentParam.FadeInTime;
				if (m_loadingUICom.CurrentParam.FadeInEvent != null) m_loadingUICom.CurrentParam.FadeInEvent.Invoke();
			}

			//背景淡入
			m_imgBackground.color = m_imgBackground.color.SetAlpha(0f);
			if (m_BGFadeInTime > 0 && m_showBackGround) {
				m_imgBackground.DOFade(1, m_BGFadeInTime).SetUpdate(true).SetEase(Ease.Linear);
			}

			var delayTime = Mathf.Max(m_BGFadeInTime, fadeInTime);
			if (delayTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(delayTime), DelayType.Realtime);
			m_imgBackground.color = m_imgBackground.color.SetAlpha(1f);

			if (m_loadingUICom is {CurrentParam: {RunningEvent: { }}}) {
				m_loadingUICom.CurrentParam.RunningEvent.Invoke();
			}
		}

		private async UniTask OnExitLoading() {
			if (m_loadFinish) return;
			m_loadFinish = true;

			m_goProgressBar.SetActive(false);
			//检测卡顿
			var frameTime = 0f;
			do {
				var start = Time.realtimeSinceStartup;
				await UniTask.Yield();
				frameTime = Time.realtimeSinceStartup - start;
			}
			//直到帧数稳定
			while (frameTime > 0.05f);

			//恢复游戏时间
			Time.timeScale = 1f;
			//淡出动画
			var fadeOutTime = 0f;
			if (m_loadingUICom is {CurrentParam: { }}) {
				fadeOutTime = m_loadingUICom.CurrentParam.FadeOutTime;
				if (m_loadingUICom.CurrentParam.FadeOutEvent != null) m_loadingUICom.CurrentParam.FadeOutEvent.Invoke();
			}

			//背景淡出
			m_imgBackground.color = m_imgBackground.color.SetAlpha(1);
			if (m_BGFadeOutTime > 0 && m_showBackGround) {
				m_imgBackground.DOFade(0, m_BGFadeOutTime).SetUpdate(true).SetEase(Ease.Linear);
			}

			var delayTime = Mathf.Max(m_BGFadeOutTime, fadeOutTime);
			if (delayTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(delayTime), DelayType.Realtime);
			m_imgBackground.color = m_imgBackground.color.SetAlpha(0);

			Log.Debug($"Open scene : {LoadSceneName}");
			GameEvent.Get<IEventScene>().OpenScene(LoadSceneName);
			UIUtils.Hide(this);
		}

		private void OnLoadingSceneProgress(float _progress) {
			if (m_loadFinish) {
				return;
			}

			var targetProgress = _progress >= 0.9f ? 1f : _progress;

			// 增加时间累计
			m_timer += Time.unscaledDeltaTime;

			// 计算插值目标：在最小加载时间内匀速过渡到 1
			var loadingTime = m_minLoadingTime;
			var maxProgressAllowed = Mathf.Min(m_timer / loadingTime, targetProgress);
			m_displayProgress = Mathf.MoveTowards(m_displayProgress, maxProgressAllowed, Time.unscaledDeltaTime * (1 / loadingTime));

			m_imgBarFg.transform.localScale = new Vector3(m_displayProgress, 1, 1);
			m_textProgress.text = (int) (m_displayProgress * 100) + " %";

			// 如果满足两个条件：时间到、进度到
			if (m_timer >= loadingTime && m_displayProgress >= 1f) {
				if (m_suspendLoad) {
					m_textProgress.text = "按任意键继续";
					foreach (var key in Keyboard.current.allKeys) {
						if (key is {wasPressedThisFrame: true}) {
							if (GameModule.Scene.UnSuspend(LoadSceneName)) {
								OnExitLoading();
								break;
							}
						}
					}
				} else {
					if (GameModule.Scene.UnSuspend(LoadSceneName)) {
						OnExitLoading();
					}
				}
			}
		}

		private void SetLoadingData(InitParam param) {
			LoadSceneName = param.SceneName;
			m_suspendLoad = param.SuspendLoad;
			m_loadingMode = param.Mode;
			m_gcCollect = param.GCCollect;
			m_showProgressBar = param.ShowProgressBar;
			m_showBackGround = param.ShowBackGround;
			m_BGFadeInTime = param.BGFadeInTime;
			m_BGFadeOutTime = param.BGFadeOutTime;

			if (m_loadingUICom) {
				m_minLoadingTime = m_loadingUICom.MinLoadingTime;
				m_loadingUICom.OnLoadingUIRefresh(param.LoadingType);
			}
		}

		protected override void OnCreate() {
			base.OnCreate();

			m_loadingUICom = gameObject.GetComponentInChildren<LoadingUICom>(true);
		}

		protected override void OnSetVisible(bool visible) {
			base.OnSetVisible(visible);
			if (m_loadingUICom) {
				m_loadingUICom.OnVisible(visible);
			}
		}

		protected override void OnRefresh() {
			base.OnRefresh();
			//init
			m_minLoadingTime = MIN_LOADING_TIME;
			if (UserData is InitParam p) {
				SetLoadingData(p);
			}

			m_displayProgress = m_timer = 0f;
			m_loadFinish = false;
			m_goProgressBar.SetActive(m_showProgressBar);
			m_imgBackground.color = m_imgBackground.color.SetAlpha(0f);
			m_imgBackground.gameObject.SetActive(m_showBackGround);

			OnEnterLoading();
		}

		public class InitParam {
			public string SceneName;
			public bool SuspendLoad = false;
			public bool GCCollect = true;
			public LoadSceneMode Mode = LoadSceneMode.Single;
			public string LoadingType = "";
			public bool ShowProgressBar = false;
			public bool ShowBackGround = false;
			public float BGFadeInTime = 0f;
			public float BGFadeOutTime = 0f;
		}
	}
}