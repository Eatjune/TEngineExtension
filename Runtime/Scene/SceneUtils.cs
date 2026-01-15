using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLogic {
	public static class SceneUtils {
		/// <summary>
		/// 使用loading异步读取场景
		/// </summary>
		/// <param name="loadSceneName">需要加载的场景名称</param>
		public static async UniTask<Scene> LoadScene(string loadSceneName) {
			return await LoadScene(loadSceneName, false, LoadSceneMode.Single);
		}

		/// <summary>
		/// 使用loading异步读取场景
		/// </summary>
		/// <param name="loadSceneName">需要加载的场景名称</param>
		/// <param name="suspendLoad">加载完毕时是否主动挂起</param>
		/// <param name="mode">场景加载模式</param>
		public static async UniTask<Scene> LoadScene(string loadSceneName, bool suspendLoad, LoadSceneMode mode) {
			await GameModule.Scene.LoadSceneAsync("Loading", LoadSceneMode.Additive);

			var loadingScene = SceneManager.GetSceneByName("Loading");
			foreach (var rootGameObject in loadingScene.GetRootGameObjects()) {
				if (rootGameObject.GetComponentInChildren<SceneCom_LoadAsyncScene>() is var loadAsync && loadAsync) {
					// loadAsync.m_inputSystemActions = new InputSystem_Actions();
					loadAsync.NextSceneName = loadSceneName;
					return await loadAsync.LoadScene(suspendLoad, mode);
				}
			}

			throw new Exception("加载Loading场景出错");
		}

		/// <summary>
		/// 静默加载场景，不显示加载场景
		/// </summary>
		/// <param name="location">场景资源定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="suspendLoad">加载完毕时是否主动挂起</param>
		public static async UniTask<Scene> LoadSceneInBackground(string loadSceneName, LoadSceneMode mode = LoadSceneMode.Additive, bool suspendLoad = false) {
			return await GameModule.Scene.LoadSceneAsync(loadSceneName, mode, suspendLoad);
		}

		/// <summary>
		/// 异步卸载子场景
		/// </summary>
		/// <param name="location">场景资源定位地址。</param>
		/// <param name="progressCallBack">进度回调。</param>
		public static async UniTask<bool> UnloadAsync(string location, Action<float> progressCallBack = null) {
			return await GameModule.Scene.UnloadAsync(location, progressCallBack);
		}

		/// <summary>
		/// 异步卸载子场景
		/// </summary>
		/// <param name="scene">场景</param>
		/// <param name="progressCallBack">进度回调。</param>
		public static async UniTask<bool> UnloadAsync(Scene scene, Action<float> progressCallBack = null) {
			return await UnloadAsync(scene.name, progressCallBack);
		}
	}
}
