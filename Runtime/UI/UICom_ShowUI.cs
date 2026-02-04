using System;
using LymeUtils.Common;
using Sirenix.OdinInspector;
using TEngine;
using UnityEngine;

namespace GameLogic {
	public class UICom_ShowUI : MonoBehaviour {
		[ValueDropdown(nameof(GetEnumOptions))]
		public string ShowUIName;

		public bool ExecuteOnStart = true;

		public void ShowUI() {
			var UIType = ClassUtils.GetType(ShowUIName, "GameLogic", "GameLogic");
			if (UIType == null) {
				Log.Error($"请检查UI名是否出错:{ShowUIName}  {gameObject.GetPath()}");
				return;
			}

			GameModule.UI.ShowUIAsync(UIType);
		}

		/// <summary>
		/// 获取所有该枚举内string
		/// </summary>
		private static string[] GetEnumOptions() {
			return ClassUtils.GetAllSubclassNames<UIWindow>();
		}

		protected void Start() {
			if (ExecuteOnStart) {
				ShowUI();
			}
		}
	}
}
