using System;
using LymeUtils.Common;
using Sirenix.OdinInspector;
using TEngine;
using UnityEngine;

namespace GameLogic {
	public class UICom_CloseUI : MonoBehaviour {
		[ValueDropdown(nameof(GetEnumOptions))]
		public string CloseUIName;

		public bool ExecuteOnStart = true;

		public void CloseUI() {
			var UIType = ClassUtils.GetType(CloseUIName, "GameLogic", "GameLogic");
			if (UIType == null) {
				Log.Error($"请检查UI名是否出错:{CloseUIName}  {gameObject.GetPath()}");
				return;
			}

			GameModule.UI.CloseUI(UIType);
		}

		/// <summary>
		/// 获取所有该枚举内string
		/// </summary>
		private static string[] GetEnumOptions() {
			return ClassUtils.GetAllSubclassNames<UIWindow>();
		}

		protected void Start() {
			if (ExecuteOnStart) {
				CloseUI();
			}
		}
	}
}
