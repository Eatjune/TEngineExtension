#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
					sourceData.Import_CSV_Advanced(string.Empty, CSVstring, mode, ',');
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
			} catch (System.Exception e) {
				Debug.LogWarning($"导出文件失败，错误：{e}");
			}
		}
	}

	public static class I2LocalizationExtensions {
		public static string Import_CSV_Advanced(this LanguageSourceData languageSourceData, string Category, string CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace,
			char Separator = ',') {
			var CSV = LocalizationReader.ReadCSV(CSVstring, Separator);
			return languageSourceData.Import_CSV_Advanced(Category, CSV, UpdateMode);
		}

		public static string Import_CSV_Advanced(this LanguageSourceData languageSourceData, string Category, List<string[]> CSV, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace) {
			string[] Tokens = CSV[0];

			int LanguagesStartIdx = 1;
			int TypeColumnIdx = -1;
			int DescColumnIdx = -1;

			var ValidColumnName_Key = new[] {"Key"};
			var ValidColumnName_Type = new[] {"Type"};
			var ValidColumnName_Desc = new[] {"Desc", "Description"};

			if (Tokens.Length > 1 && ArrayContains(Tokens[0], ValidColumnName_Key)) {
				if (UpdateMode == eSpreadsheetUpdateMode.Replace) languageSourceData.ClearAllData();

				if (Tokens.Length > 2) {
					if (ArrayContains(Tokens[1], ValidColumnName_Type)) {
						TypeColumnIdx = 1;
						LanguagesStartIdx = 2;
					}

					if (ArrayContains(Tokens[1], ValidColumnName_Desc)) {
						DescColumnIdx = 1;
						LanguagesStartIdx = 2;
					}
				}

				if (Tokens.Length > 3) {
					if (ArrayContains(Tokens[2], ValidColumnName_Type)) {
						TypeColumnIdx = 2;
						LanguagesStartIdx = 3;
					}

					if (ArrayContains(Tokens[2], ValidColumnName_Desc)) {
						DescColumnIdx = 2;
						LanguagesStartIdx = 3;
					}
				}
			} else return "Bad Spreadsheet Format.\nFirst columns should be 'Key', 'Type' and 'Desc'";

			int nLanguages = Mathf.Max(Tokens.Length - LanguagesStartIdx, 0);
			int[] LanIndices = new int[nLanguages];
			for (int i = 0; i < nLanguages; ++i) {
				if (string.IsNullOrEmpty(Tokens[i + LanguagesStartIdx])) {
					LanIndices[i] = -1;
					continue;
				}

				string langToken = Tokens[i + LanguagesStartIdx].Trim();

				string LanName, LanCode;
				bool isLangEnabled = true;
				if (langToken.StartsWith("$", StringComparison.Ordinal)) {
					isLangEnabled = false;
					langToken = langToken.Substring(1);
				}

				GoogleLanguages.UnPackCodeFromLanguageName(langToken, out LanName, out LanCode);

				int LanIdx = -1;
				if (!string.IsNullOrEmpty(LanCode)) LanIdx = languageSourceData.GetLanguageIndexFromCode(LanCode);
				else LanIdx = languageSourceData.GetLanguageIndex(LanName, SkipDisabled: false);

				if (LanIdx < 0) {
					LanguageData lanData = new LanguageData();
					lanData.Name = LanName;
					lanData.Code = LanCode;
					lanData.Flags = (byte)(0 | (isLangEnabled ? 0 : (int)eLanguageDataFlags.DISABLED));
					languageSourceData.mLanguages.Add(lanData);
					LanIdx = languageSourceData.mLanguages.Count - 1;
				}

				LanIndices[i] = LanIdx;
			}

			//--[ Update the Languages array in the existing terms]-----
			nLanguages = languageSourceData.mLanguages.Count;
			for (int i = 0, imax = languageSourceData.mTerms.Count; i < imax; ++i) {
				TermData termData = languageSourceData.mTerms[i];
				if (termData.Languages.Length < nLanguages) {
					Array.Resize(ref termData.Languages, nLanguages);
					Array.Resize(ref termData.Flags, nLanguages);
				}
			}

			//--[ Keys ]--------------

			for (int i = 1, imax = CSV.Count; i < imax; ++i) {
				Tokens = CSV[i];
				string sKey = string.IsNullOrEmpty(Category) ? Tokens[0] : string.Concat(Category, "/", Tokens[0]);

				string specialization = null;
				if (sKey.EndsWith("]", StringComparison.Ordinal)) {
					int idx = sKey.LastIndexOf('[');
					if (idx > 0) {
						specialization = sKey.Substring(idx + 1, sKey.Length - idx - 2);
						if (specialization == "touch") specialization = "Touch";
						sKey = sKey.Remove(idx);
					}
				}

				LanguageSourceData.ValidateFullTerm(ref sKey);
				if (string.IsNullOrEmpty(sKey)) continue;

				TermData termData = languageSourceData.GetTermData(sKey);

				// Check to see if its a new term
				if (termData == null) {
					termData = new TermData();
					termData.Term = sKey;

					termData.Languages = new string[languageSourceData.mLanguages.Count];
					termData.Flags = new byte[languageSourceData.mLanguages.Count];
					for (int j = 0; j < languageSourceData.mLanguages.Count; ++j) termData.Languages[j] = string.Empty;

					languageSourceData.mTerms.Add(termData);
					languageSourceData.mDictionary.Add(sKey, termData);
				} else
					// This term already exist
				if (UpdateMode == eSpreadsheetUpdateMode.AddNewTerms) continue;

				if (TypeColumnIdx > 0) termData.TermType = LanguageSourceData.GetTermType(Tokens[TypeColumnIdx]);

				if (DescColumnIdx > 0) termData.Description = Tokens[DescColumnIdx];

				for (int j = 0; j < LanIndices.Length && j < Tokens.Length - LanguagesStartIdx; ++j)
					if (!string.IsNullOrEmpty(Tokens[j + LanguagesStartIdx])) // Only change the translation if there is a new value
					{
						var lanIdx = LanIndices[j];
						if (lanIdx < 0) continue;
						var value = Tokens[j + LanguagesStartIdx];

						if (value == "-") value = string.Empty;
						else if (value == "") value = null;

						termData.SetTranslation(lanIdx, value, specialization);
					}
			}

			if (Application.isPlaying) {
				languageSourceData.SaveLanguages(languageSourceData.HasUnloadedLanguages());
			}

			return string.Empty;
		}

		private static bool ArrayContains(string MainText, params string[] texts) {
			for (int i = 0, imax = texts.Length; i < imax; ++i)
				if (MainText.IndexOf(texts[i], StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			return false;
		}
	}
}
#endif
