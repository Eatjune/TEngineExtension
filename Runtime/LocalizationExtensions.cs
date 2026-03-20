using System.Text.RegularExpressions;
using TEngine;

namespace GameLogic {
	public static class LocalizationExtensions {
		/// <summary>
		/// 获取本地化值
		/// </summary>
		public static string GetString(this ILocalizationModule localizationModule, string key) {
			return LocalizationUtils.GetString(key);
		}

		/// <summary>
		/// 是否有该key
		/// </summary>
		public static bool HasString(this ILocalizationModule localizationModule, string key) {
			return LocalizationUtils.HasString(key);
		}

		/// <summary>
		/// 格式化本地化值
		/// </summary>
		public static string FormatLocalized(this ILocalizationModule localizationModule, string key, params object[] args) {
			var content = GameModule.Localization.GetString(key.ToString());
			return Regex.Replace(content, @"@(\d+)", match => {
				var index = int.Parse(match.Groups[1].Value) - 1;
				if (index >= 0 && index < args.Length) return args[index].ToString();
				return match.Value;
			});
		}
	}
}