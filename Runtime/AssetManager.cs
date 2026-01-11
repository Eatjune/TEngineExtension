using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LymeUtils.Config;
using TEngine;
using YooAsset;

namespace GameLogic {
	/// <summary>
	/// 资源数据管理
	/// </summary>
	public class AssetManager : Singleton<AssetManager> {
		private Dictionary<object, List<AssetHandle>> m_assetHandleDic = new Dictionary<object, List<AssetHandle>>();

		/// <summary>
		/// 读取资源
		/// </summary>
		public async UniTask<AssetHandle> LoadAssetAsync<T>(string name, object source) where T : UnityEngine.Object {
			source ??= this;
			var hasAssetResult = GameModule.Resource.HasAsset(name);
			if (hasAssetResult is HasAssetResult.NotExist or HasAssetResult.Valid) {
				Log.Error($"AssetManager LoadAssetAsync {name} is Null");
				return null;
			}

			List<AssetHandle> loadAssetHandles = null;
			m_assetHandleDic.TryGetValue(source, out loadAssetHandles);

			if (loadAssetHandles == null) {
				loadAssetHandles = new List<AssetHandle>();
				m_assetHandleDic.TryAdd(source, loadAssetHandles);
			}

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						return assetHandle;
					}
				}
			}

			var loadAssetHandle = GameModule.Resource.LoadAssetAsyncHandle<T>(name);
			try {
				await loadAssetHandle.Task;
			}
			catch (Exception e) {
				throw new Exception($"AssetManager LoadAssetAsyncHandle Failed : {e.Message}");
			}

			if (loadAssetHandle.AssetObject == null) {
				Log.Error($"AssetManager LoadAssetAsyncHandle {name} is Null");
				return null;
			}

			loadAssetHandles.Add(loadAssetHandle);

			return loadAssetHandle;
		}

		/// <summary>
		/// 通过枚举读取资源配置库
		/// </summary>
		public async UniTask<AssetHandle> LoadConfigAsync<T>() where T : System.Enum {
			return await LoadConfigAsync<T>(this);
		}

		/// <summary>
		/// 通过枚举读取资源配置库
		/// </summary>
		public async UniTask<AssetHandle> LoadConfigAsync<T>(object source) where T : System.Enum {
			source ??= this;
			var name = ConfigPathUtils.GetConfigDataListPathByEnum<T>();
			if (string.IsNullOrEmpty(name)) {
				Log.Error($"AssetManager LoadConfigAsync {name} is Null");
				return null;
			}

			var hasAssetResult = GameModule.Resource.HasAsset(name);
			if (hasAssetResult is HasAssetResult.NotExist or HasAssetResult.Valid) {
				Log.Error($"AssetManager LoadConfigAsync {name} is Null");
				return null;
			}

			List<AssetHandle> loadAssetHandles = null;
			m_assetHandleDic.TryGetValue(source, out loadAssetHandles);

			if (loadAssetHandles == null) {
				loadAssetHandles = new List<AssetHandle>();
				m_assetHandleDic.TryAdd(source, loadAssetHandles);
			}

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						return assetHandle;
					}
				}
			}

			var loadAssetHandle = GameModule.Resource.LoadAssetAsyncHandle<ConfigDatabase>(name);
			try {
				await loadAssetHandle.Task;
			}
			catch (Exception e) {
				throw new Exception($"AssetManager LoadConfigAsync Failed : {e.Message}");
			}

			if (loadAssetHandle.AssetObject == null) {
				Log.Error($"AssetManager LoadConfigAsync {name} is Null");
				return null;
			}

			loadAssetHandles.Add(loadAssetHandle);

			return loadAssetHandle;
		}

		/// <summary>
		/// 释放对象上所有资源
		/// </summary>
		public void UnLoadAllAssetInObject(object source) {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);
			if (loadAssetHandles is {Count: > 0}) {
				foreach (var loadAssetHandle in loadAssetHandles) {
					loadAssetHandle.Release();
				}
			}

			m_assetHandleDic.Remove(source);
		}

		/// <summary>
		/// 释放对象上资源
		/// </summary>
		public void UnloadAssetInObject(object source, string name) {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);
			if (loadAssetHandles is {Count: > 0}) {
				foreach (var loadAssetHandle in loadAssetHandles) {
					if (loadAssetHandle.IsDone && loadAssetHandle.AssetObject.name == name) {
						loadAssetHandle.Release();
						return;
					}
				}
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void UnloadAsset(string name) {
			foreach (var assetHandleList in m_assetHandleDic.Values) {
				foreach (var assetHandle in assetHandleList) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						assetHandle.Release();
						return;
					}
				}
			}
		}

		/// <summary>
		/// 获取资源
		/// </summary>
		public T GetAsset<T>(string name) where T : UnityEngine.Object {
			foreach (var assetHandleList in m_assetHandleDic.Values) {
				foreach (var assetHandle in assetHandleList) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						return assetHandle.AssetObject as T;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 获取资源
		/// </summary>
		public T GetAsset<T>(object source) where T : UnityEngine.Object {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject is T assetObject) {
						return assetObject;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 获取资源
		/// </summary>
		public T GetAsset<T>(string name, object source) where T : UnityEngine.Object {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						return assetHandle.AssetObject as T;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 读取资源
		/// </summary>
		public object GetAsset(string name, object source) {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
						return assetHandle.AssetObject;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 获取资源
		/// </summary>
		public object GetAsset(string name, Type type, object source) {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.GetType() == type && assetHandle.AssetObject.name == name) {
						return assetHandle.AssetObject;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 获取资源
		/// </summary>
		public object GetAsset(Type type, object source) {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);

			if (loadAssetHandles is {Count: > 0}) {
				foreach (var assetHandle in loadAssetHandles) {
					if (assetHandle.IsDone && assetHandle.AssetObject.GetType() == type) {
						return assetHandle.AssetObject;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 获取资源，如果没有则加载
		/// </summary>
		/// <param name="name">资源名</param>
		/// <param name="source">脚本源,一般是this</param>
		public async UniTask<T> GetAssetOrLoad<T>(string name, object source) where T : UnityEngine.Object {
			var asset = GetAsset<T>(name);
			if (!asset) {
				var hasAssetResult = GameModule.Resource.HasAsset(name);
				if (!(hasAssetResult is HasAssetResult.NotExist or HasAssetResult.Valid)) {
					source ??= this;
					var assetHandle = await LoadAssetAsync<T>(name, source);
					if (assetHandle.IsDone) {
						asset = assetHandle.AssetObject as T;
					}
				}
			}

			return asset;
		}

		/// <summary>
		/// 获取资源，如果没有则加载
		/// </summary>
		/// <param name="type">类型</param>
		/// <param name="name">资源名</param>
		/// <param name="source">脚本源,一般是this</param>
		public async UniTask<object> GetAssetOrLoad(Type type, string name, object source) {
			source ??= this;
			var methodInfo = typeof(AssetManager).GetMethod(nameof(GetAssetOrLoad), BindingFlags.Instance | BindingFlags.Public, binder: null,
				types: new[] {typeof(string), typeof(object)}, modifiers: null);
			var genericMethod = methodInfo.MakeGenericMethod(type);
			var taskObj = genericMethod.Invoke(this, new object[] {name, source});

			var castMethod = this.GetType().GetMethod(nameof(CastToObject), BindingFlags.NonPublic | BindingFlags.Instance) !.MakeGenericMethod(type);
			var castedTask = (UniTask<object>) castMethod.Invoke(this, new object[] {taskObj});

			return await castedTask;
		}

		/// <summary>
		/// 获取资源，如果没有则加载
		/// </summary>
		/// <param name="name">资源名</param>
		public async UniTask<T> GetAssetOrLoad<T>(string name) where T : UnityEngine.Object {
			return await GetAssetOrLoad<T>(name, null);
		}

		/// <summary>
		/// 通过枚举读取资源（配置）
		/// </summary>
		public ConfigDatabase GetConfig<T>(object source) where T : System.Enum {
			source ??= this;
			m_assetHandleDic.TryGetValue(source, out var loadAssetHandles);
			if (loadAssetHandles == null) return null;
			var name = ConfigUtils.GetConfigDataNameByEnum<T>();
			foreach (var assetHandle in loadAssetHandles) {
				if (assetHandle.IsDone && assetHandle.AssetObject.name == name) {
					return assetHandle.AssetObject as ConfigDatabase;
				}
			}

			return null;
		}

		/// <summary>
		/// 通过枚举读取资源（配置）
		/// </summary>
		public ConfigDatabase GetConfig<T>() where T : System.Enum {
			return GetConfig<T>(this);
		}

		private async UniTask<object> CastToObject<T>(UniTask<T> task) {
			var result = await task;
			return (object) result;
		}
	}
}