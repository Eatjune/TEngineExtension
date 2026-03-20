using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LymeUtils.Common;
using Mono.CSharp;
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameLogic {
	public class UINavigateSystem : Singleton<UINavigateSystem>, IUpdate {
		private readonly List<RaycastResult> _raycastResults = new();
		public GameObject CurrentFocusObject { get; private set; }

		public bool UINavigateActive { get; private set; } = false;

		/// <summary>
		/// 导航界面
		/// </summary>
		public Transform NavigateUI;

		public void ActiveNavigate(Transform uiTransform = null) {
			UINavigateActive = true;
			NavigateUI = uiTransform;
		}

		public void InActiveNavigate() {
			UINavigateActive = false;
			NavigateUI = null;
		}

		/// <summary>
		/// 选中最右下角的 UI 元素
		/// </summary>
		private void SelectBottomRightUI(Transform root = null) {
			var selectables = root == null ? Selectable.allSelectablesArray : root.GetComponentsInChildren<Selectable>();

			Selectable bottomRight = null;
			var maxScore = float.MinValue;

			foreach (var selectable in selectables) {
				if (!selectable.IsInteractable() || !selectable.gameObject.activeSelf) continue;

				var rectTransform = selectable.GetComponent<RectTransform>();
				if (rectTransform == null) continue;

				var screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
				var score = screenPos.x - screenPos.y;

				if (score > maxScore) {
					maxScore = score;
					bottomRight = selectable;
				}
			}

			if (bottomRight != null) {
				EventSystem.current.SetSelectedGameObject(bottomRight.gameObject);
			}
		}

		public void OnUpdate() {
			UpdateHoverFocus();
			// HandleNavigate();
			HandleConfirm();
		}

		/// <summary>
		/// 更新焦点位置
		/// </summary>
		private void UpdateHoverFocus() {
			if (Mouse.current == null) return;

			//鼠标移动时显示鼠标
			var mouseDelta = Mouse.current.delta.ReadValue();
			if (mouseDelta.sqrMagnitude > 0.01f) {
				Cursor.visible = true;
			}

			if (!Cursor.visible) return;

			var eventData = new PointerEventData(EventSystem.current) {
				position = Mouse.current.position.ReadValue()
			};
			_raycastResults.Clear();
			EventSystem.current.RaycastAll(eventData, _raycastResults);
			if (_raycastResults.Count > 0) {
				foreach (var result in _raycastResults) {
					if (result.gameObject == null) continue;
					var selectableCom = result.gameObject.GetComponentInParent<Selectable>();
					if (selectableCom == null) continue;

					// CurrentFocusObject = selectableCom.gameObject;
					//设置焦点对象
					EventSystem.current.SetSelectedGameObject(selectableCom.gameObject);
					break;
				}
			}
		}

		private void HandleConfirm() {
			if ((CurrentFocusObject != null || EventSystem.current.currentSelectedGameObject != null) && InputSystem.Instance.Actions.UI.Submit.WasPressedThisFrame()) {
				var go = CurrentFocusObject != null ? CurrentFocusObject : EventSystem.current.currentSelectedGameObject;
				var eventData = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(go, eventData, ExecuteEvents.pointerClickHandler);
			}
		}

		private async void OnNavigate(InputAction.CallbackContext ctx) {
			if (!UINavigateActive) return;
			Cursor.visible = false;
			//如果为空，则获取最顶层ui
			if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy) {
				await UniTask.Yield();
				if (NavigateUI != null) {
					SelectBottomRightUI(NavigateUI);
				}
				//没有导航UI则获取最顶层UI
				else {
					var topWindowFullName = GameModule.UI.GetTopWindow();
					var index = topWindowFullName.LastIndexOf('.');
					var topWindowName = index >= 0 ? topWindowFullName[(index + 1)..] : topWindowFullName;
					var topWindowObj = GameModule.UI.UICamera.transform.parent.Find($"UICanvas/{topWindowName}");
					if (topWindowObj != null) {
						SelectBottomRightUI(topWindowObj.transform);
					}
				}
			}
		}

		protected override void OnInit() {
			InputSystem.Instance.Actions.UI.Navigate.performed += OnNavigate;
		}

		protected override void OnRelease() {
			InputSystem.Instance.Actions.UI.Navigate.performed -= OnNavigate;
		}

		private float m_lastNavigateTime;

		private void HandleNavigate() {
			if (Time.realtimeSinceStartup - m_lastNavigateTime <= 0.2f) return;
			var navigate = InputSystem.Instance.Actions.UI.Navigate.ReadValue<Vector2>();
			if (navigate == Vector2.zero) return;

			//如果为空，则获取最顶层ui
			if (EventSystem.current.currentSelectedGameObject == null) {
				var topWindowFullName = GameModule.UI.GetTopWindow();
				var index = topWindowFullName.LastIndexOf('.');
				var topWindowName = index >= 0 ? topWindowFullName[(index + 1)..] : topWindowFullName;
				var topWindowObj = GameModule.UI.UICamera.transform.parent.Find($"UICanvas/{topWindowName}");
				if (topWindowObj != null) {
					SelectBottomRightUI(topWindowObj.transform);
				}
			}

			if (EventSystem.current.currentSelectedGameObject == null) return;

			var selectable = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Selectable>();
			if (selectable == null) return;

			Selectable next = null;
			if (navigate.y > 0.5f) next = selectable.FindSelectableOnUp();
			else if (navigate.y < -0.5f) next = selectable.FindSelectableOnDown();
			else if (navigate.x < -0.5f) next = selectable.FindSelectableOnLeft();
			else if (navigate.x > 0.5f) next = selectable.FindSelectableOnRight();

			if (next != null) {
				EventSystem.current.SetSelectedGameObject(next.gameObject);
				m_lastNavigateTime = Time.realtimeSinceStartup;
			}
		}
	}
}