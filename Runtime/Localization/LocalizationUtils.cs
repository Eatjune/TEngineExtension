namespace GameLogic {
	public static class LocalizationUtils {
		/// <summary>
		/// 获取本地化值
		/// </summary>
		public static string GetString(string key) {
			var content = TEngine.Localization.LocalizationManager.GetTranslation(key);
			if (string.IsNullOrEmpty(content)) {
				content = key;
			}

			return content;
		}

		/// <summary>
		/// 是否有该key
		/// </summary>
		public static bool HasString(string key) {
			var content = TEngine.Localization.LocalizationManager.GetTranslation(key);
			if (string.IsNullOrEmpty(content)) {
				return false;
			}

			return true;
		}
	}
}