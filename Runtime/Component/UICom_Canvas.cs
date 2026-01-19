using System;
using System.Collections.Generic;
using LymeUtils.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic {
	public class UICom_Canvas : MonoBehaviour {
		public bool OverrideSorting = false;

		[ShowIf("@OverrideSorting")]
		[ValueDropdown(nameof(GetSortingLayers))]
		public string sortingLayer = "Default";

		[ShowIf("@OverrideSorting")]
		public int SortingOder = 0;

		public bool AddGraphicRaycaster = true;

		public Canvas Canvas {
			get {
				if (m_canvas == null) CreateCanvas();
				return m_canvas;
			}
		}

		protected Canvas m_canvas;

		public GraphicRaycaster GraphicRaycaster {
			get {
				if (m_canvas == null) CreateCanvas();
				return m_graphicRaycaster;
			}
		}

		protected GraphicRaycaster m_graphicRaycaster;

		private void CreateCanvas() {
			if (m_canvas != null) return;
			var _canvas = gameObject.GetOrAddComponent<Canvas>();
			_canvas.overrideSorting = OverrideSorting;
			_canvas.sortingLayerName = sortingLayer;
			_canvas.sortingOrder = SortingOder;
			if (AddGraphicRaycaster) {
				m_graphicRaycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();
			}

			m_canvas = _canvas;
		}

		protected void Start() {
			CreateCanvas();
		}
#if UNITY_EDITOR
		private static IEnumerable<string> GetSortingLayers() {
			foreach (var layer in SortingLayer.layers) {
				yield return layer.name;
			}
		}

#endif
	}
}