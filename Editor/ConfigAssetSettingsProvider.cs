#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameLogic;
using LymeUtils.Config;
using UnityEditor;
using UnityEngine;

namespace TEngineExtension.Editor {
	public static class ConfigAssetSettingsProvider {
		public static string SettingTitle = "ConfigAsset";

		private static ConfigAssetSObject _settings;
		private static SerializedObject _serializedSettings;

		private static string[] _configImplementationFullNames;
		private static string[] _configImplementationLabels;

		static ConfigAssetSettingsProvider() {
			InitializeConfigDataOptions();
		}

		[SettingsProvider]
		public static SettingsProvider PackageProvider() {
			var provider = new SettingsProvider($"Project/LymeGame/{SettingTitle}", SettingsScope.Project) {
				label = SettingTitle,
				guiHandler = OnPackageGUIHandler,
				activateHandler = (searchContext, rootElement) => {
					_settings = ConfigAssetUtils.LoadOrCreateConfig();
					if (_settings != null) {
						_serializedSettings = new SerializedObject(_settings);
					}

					// 每次激活面板时重新扫描一次，以防新写了脚本没刷新
					InitializeConfigDataOptions();
				},
			};

			return provider;
		}

		public static void OnPackageGUIHandler(string searchContext) {
			if (_settings == null) {
				_settings = ConfigAssetUtils.LoadOrCreateConfig();
				if (_settings != null) _serializedSettings = new SerializedObject(_settings);
			}

			if (_settings == null) {
				EditorGUILayout.HelpBox("Failed to load or create ConfigAssetSObject asset.", MessageType.Error);
				return;
			}

			_serializedSettings.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("PreLoad Configuration", EditorStyles.boldLabel);

			if (_configImplementationFullNames == null || _configImplementationFullNames.Length == 0) {
				EditorGUILayout.HelpBox("No classes inheriting from ConfigData found in the project!", MessageType.Warning);
			} else {
				DrawConfigDataList(_settings.PreLoadTypeNames);
			}

			if (GUI.changed) {
				EditorUtility.SetDirty(_settings);
				_serializedSettings.ApplyModifiedProperties();
			}
		}

		private static void InitializeConfigDataOptions() {
			var targetType = typeof(ConfigData);

			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies) {
				try {
					var assemblyTypes = assembly.GetTypes();
					foreach (var t in assemblyTypes) {
						if (targetType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract) {
							types.Add(t);
						}
					}
				} catch (ReflectionTypeLoadException e) {
					foreach (var t in e.Types) {
						if (t != null && targetType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract) {
							types.Add(t);
						}
					}
				} catch (Exception) {
					continue;
				}
			}

			// 排序
			types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

			_configImplementationFullNames = types.Select(t => t.AssemblyQualifiedName).ToArray();
			_configImplementationLabels = types.Select(t => t.FullName).ToArray();
		}

		/// <summary>
		/// 绘制下拉框列表
		/// </summary>
		private static void DrawConfigDataList(List<string> typeNames) {
			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"ConfigData Types: {typeNames.Count}");
			if (GUILayout.Button("+", GUILayout.Width(30))) {
				var defaultType = _configImplementationFullNames.Length > 0 ? _configImplementationFullNames[0] : "";
				typeNames.Add(defaultType);
			}

			EditorGUILayout.EndHorizontal();

			for (var i = 0; i < typeNames.Count; i++) {
				EditorGUILayout.BeginHorizontal();

				var currentStoredValue = typeNames[i];

				var selectedIndex = Array.IndexOf(_configImplementationFullNames, currentStoredValue);

				if (selectedIndex < 0) {
					selectedIndex = 0;
				}

				var newSelectedIndex = EditorGUILayout.Popup($"Item {i + 1}", selectedIndex, _configImplementationLabels);

				if (newSelectedIndex != selectedIndex || (!string.IsNullOrEmpty(currentStoredValue) && selectedIndex == 0 && currentStoredValue != _configImplementationFullNames[0])) {
					if (newSelectedIndex >= 0 && newSelectedIndex < _configImplementationFullNames.Length) {
						typeNames[i] = _configImplementationFullNames[newSelectedIndex];
					}
				}

				if (GUILayout.Button("-", GUILayout.Width(30))) {
					typeNames.RemoveAt(i);
					i--;
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}
	}
#endif
}
