using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic {
	/// <summary>
	/// 资源索引文件
	/// </summary>
	public static class ConfigAsset {
		public static string ConfigAssetPath => $"Assets/Resources/{ConfigAssetResourcesPath}.asset";

		public static string ConfigAssetResourcesPath = "LymeGame/Config/ConfigAsset";

		private static Type[] _preLoadAssets;

		/// <summary>
		/// 预先加载配置资源类型
		/// </summary>
		public static Type[] PreLoadAssets {
			get {
				if (_preLoadAssets == null) {
					LoadConfig();
				}

				return _preLoadAssets;
			}
		}

		private static void LoadConfig() {
			var settings = Resources.Load<ConfigAssetSObject>(ConfigAssetResourcesPath);

			if (settings != null && settings.PreLoadTypeNames != null) {
				var types = new List<Type>();
				foreach (var typeName in settings.PreLoadTypeNames) {
					if (!string.IsNullOrEmpty(typeName)) {
						var t = Type.GetType(typeName);
						if (t != null) {
							types.Add(t);
						} else {
							Debug.LogError($"[ConfigAsset] Could not find type: {typeName}");
						}
					}
				}

				_preLoadAssets = types.ToArray();
			} else {
				Debug.LogError("[ConfigAsset] ConfigAssetSettings not found in Resources!");
				_preLoadAssets = new Type[0];
			}
		}
	}
}
