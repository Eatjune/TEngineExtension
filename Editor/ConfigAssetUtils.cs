#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameLogic {
	public static class ConfigAssetUtils {
		private static ConfigAssetSObject _config;

		public static ConfigAssetSObject Config {
			get {
				if (_config == null) {
					_config = LoadOrCreateConfig();
				}

				return _config;
			}
		}

		public static ConfigAssetSObject LoadOrCreateConfig() {
#if UNITY_EDITOR
			var config = AssetDatabase.LoadAssetAtPath<ConfigAssetSObject>(ConfigAsset.ConfigAssetPath);
			if (config == null) {
				config = ScriptableObject.CreateInstance<ConfigAssetSObject>();
				var folderPath = Path.GetDirectoryName(ConfigAsset.ConfigAssetPath);
				if (!Directory.Exists(folderPath)) {
					Directory.CreateDirectory(folderPath);
				}

				AssetDatabase.CreateAsset(config, ConfigAsset.ConfigAssetPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"[{nameof(ConfigAssetSObject)}] Created new configuration asset at {ConfigAsset.ConfigAssetPath}");
			}

			return config;
#else
            if (_config == null) {
                _config = Resources.LoadAssetAtPath<ConfigAssetSObject>(ConfigAsset.ConfigAssetPath);
            }
            return _config;
#endif
		}
	}
}
#endif
