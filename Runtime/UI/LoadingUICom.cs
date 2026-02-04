using System;
using System.Linq;
using LymeUtils.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GameLogic {
	public class LoadingUICom : MonoBehaviour {
		[BoxGroup("当前参数"), SerializeField, HideInEditorMode]
		public LoadingTypeParam CurrentParam;

		[BoxGroup("默认参数"), SerializeField]
		public LoadingTypeParam DefaultParam;

		[LabelText("参数列表"), SerializeField]
		public LoadingTypeParam[] Types;

		public void SetType(string key) {
			if (Types is {Length: > 0}) {
				foreach (var loadingTypeParam in Types) {
					loadingTypeParam.RootObject.SetActive(false);
				}
			}

			var typeParam = Types.Where(p => p.Key == key).ToList();
			if (typeParam.Count > 0) {
				CurrentParam = typeParam[RandomUtils.Range(0, typeParam.Count)];
				CurrentParam.RootObject.SetActive(true);
			}
		}

		public void OnVisible(bool visible) {
			if (visible) {
				if (CurrentParam == null && DefaultParam is {RootObject: { }}) {
					CurrentParam = DefaultParam;
				}
			} else {
				CurrentParam = null;
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
			public UnityEvent FadeOutEvent;
			public float FadeInTime;
			public float FadeOutTime;
		}
	}
}
