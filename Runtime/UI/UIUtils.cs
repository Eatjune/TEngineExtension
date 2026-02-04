using System;
using LymeUtils.Common;

namespace GameLogic {
	public static class UIUtils {
		public static void Hide(UIWindow window, int hideTimeToClose = -1) {
			if (window == null) {
				return;
			}

			window.HideTimeToClose = hideTimeToClose;
			if (window.HideTimeToClose == 0) {
				GameModule.UI.CloseUI(window.GetType());
				return;
			}

			window.CancelHideToCloseTimer();
			window.Visible = false;
			window.IsHide = true;
			if (window.HideTimeToClose > 0) {
				window.HideTimerId = GameModule.Timer.AddTimer((arg) => {
					GameModule.UI.CloseUI(window.GetType());
				}, window.HideTimeToClose);
			}

			if (window.FullScreen) {
				ClassUtils.CallMethod(GameModule.UI, "OnSetWindowVisible");
			}
		}
	}
}
