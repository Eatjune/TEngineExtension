using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LymeUtils.Config;
using UnityEngine;

namespace GameLogic {
	public class ConfigManager : Singleton<ConfigManager> {
		private List<ConfigData> m_configData = new List<ConfigData>();

		public static T Get<T>() where T : ConfigData {
			foreach (var scriptableObject in _instance.m_configData) {
				if (scriptableObject is T o) {
					return o;
				}
			}

			return null;
		}

		public async UniTask PreLoadConfigData() {
			if (ConfigAsset.PreLoadAssets is {Length: > 0}) {
				var tasks = new List<UniTask<object>>();
				foreach (var preLoadAsset in ConfigAsset.PreLoadAssets) {
					var task = AssetManager.Instance.GetAssetOrLoad(preLoadAsset, preLoadAsset.Name, this);
					tasks.Add(task);
				}

				var results = await UniTask.WhenAll(tasks);

				foreach (var config in results) {
					if (config is ConfigData data) {
						m_configData.Add(data);
					}
				}
			}

			Debug.Log($"ConfigManager所有配置读取完毕");
		}
	}
}
