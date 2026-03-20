#if UNITY_EDITOR
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using LymeUtils.Common;
using TEngine.Localization;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor {
	public class I2LocalizationEditor {
		[MenuItem("Tools/I2 Localization/一键导出", false, 30)]
		public async static void Import2ExportAllCSV() {
			try {
				TEngine.Localization.LocalizationManager.InitializeIfNeeded();
				var sourceData = TEngine.Localization.LocalizationManager.Sources[0];
				var files = Directory.GetFiles(Application.dataPath + "/AssetRaw/Configs/Localization/Import", "*.csv");
				var isReplace = true;
				foreach (var file in files) {
					Debug.Log($"读取文件:{file}..");
					var CSVstring = File.ReadAllText(file);
					var mode = isReplace ? eSpreadsheetUpdateMode.Replace : eSpreadsheetUpdateMode.Merge;
					sourceData.Import_CSV(string.Empty, CSVstring, mode, ',');
					isReplace = false;
				}

				EditorUtility.SetDirty(sourceData.ownerObject);
				AssetDatabase.SaveAssets();
				await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
				Debug.Log($"整合文件完毕，重新加载域中..");
				TEngine.Localization.LocalizationManager.LocalizeAll(); // Force localing all enabled labels/sprites with the new data

				await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
				//导出
				char Separator = sourceData.Spreadsheet_LocalCSVSeparator.Length > 0 ? sourceData.Spreadsheet_LocalCSVSeparator[0] : ',';
				string exportCSVstring = sourceData.Export_CSV(null, Separator, sourceData.Spreadsheet_SpecializationAsRows, sourceData.Spreadsheet_SortRows);
				var encoding = System.Text.Encoding.GetEncoding(sourceData.Spreadsheet_LocalCSVEncoding);
				File.WriteAllText(Application.dataPath + "/AssetRaw/Configs/Localization/Export/Localization_export.csv", exportCSVstring, encoding);

				await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

				ManuallyReloadDomainTool.ForceReloadDomain();
				Debug.Log($"重新加载域完毕,导出文件成功");
			}

			catch (System.Exception e) {
				Debug.LogWarning($"导出文件失败，错误：{e}");
			}
		}
	}
}
#endif