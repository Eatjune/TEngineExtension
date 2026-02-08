using System;
using System.Linq;
using LymeUtils.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GameLogic {
	public class LoadingUICom : MonoBehaviour {
		/// <summary>
		/// 最少读取秒
		/// </summary>
		public float MinLoadingTime = 0.3f;

		[BoxGroup("当前参数"), SerializeField, HideInEditorMode]
		public LoadingTypeParam CurrentParam;

		[BoxGroup("默认参数"), SerializeField]
		public LoadingTypeParam DefaultParam;

		[LabelText("参数列表"), SerializeField]
		public LoadingTypeParam[] Types;

		public void OnLoadingUIRefresh(string typeKey) {
			if (Types is {Length: > 0}) {
				foreach (var loadingTypeParam in Types) {
					loadingTypeParam.RootObject.SetActive(false);
				}
			}

			//设置默认参数
			if (CurrentParam == null || CurrentParam.RootObject == null && DefaultParam != null && DefaultParam.RootObject != null) {
				CurrentParam = DefaultParam;
			}

			//如果类型不为空，则设置类型
			if (!string.IsNullOrEmpty(typeKey) && Types is {Length: > 0}) {
				var typeParam = Types.Where(p => p.Key == typeKey).ToList();
				if (typeParam.Count > 0) {
					CurrentParam = typeParam[RandomUtils.Range(0, typeParam.Count)];
				}
			}

			if (CurrentParam != null && CurrentParam.RootObject != null) {
				CurrentParam.RootObject.SetActive(true);
			}
		}

		public void OnVisible(bool visible) {
			if (visible) {
			} else {
				CurrentParam = null;
				if (DefaultParam is {RootObject: { }}) {
					DefaultParam.RootObject.SetActive(false);
				}

				if (Types is {Length: > 0}) {
					foreach (var loadingTypeParam in Types) {
						loadingTypeParam.RootObject.SetActive(false);
					}
				}
			}
		}

		private void Awake() {
			CurrentParam = null;

			//设置默认值
			if (DefaultParam is {RootObject: { }}) {
				DefaultParam.RootObject.SetActive(false);
				CurrentParam = DefaultParam;
			}

			if (Types is {Length: > 0}) {
				foreach (var loadingTypeParam in Types) {
					loadingTypeParam.RootObject.SetActive(false);
				}
			}
		}

		[Serializable]
		public class LoadingTypeParam {
			public string Key;
			public GameObject RootObject;
			public UnityEvent FadeInEvent;
			public UnityEvent RunningEvent;
			public UnityEvent FadeOutEvent;
			public float FadeInTime;
			public float FadeOutTime;
		}
	}
}