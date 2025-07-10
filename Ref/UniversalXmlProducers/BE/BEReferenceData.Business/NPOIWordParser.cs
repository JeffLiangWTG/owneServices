using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.HWPF;
using NPOI.HWPF.Extractor;
using NPOI.HWPF.UserModel;
using NPOI.XWPF.UserModel;
using static CargoWise.RefDbRepo.BEReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class NPOIWordParser
	{
		public static List<RefCusCodeList> ReadAdditionalInfoDocFileIntoResults(List<string> fileNames, List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex)
		{
			return ReadDocFilesIntoResults(fileNames, codeHeaders, descriptionHeaders, codeRegex, new List<string> { "ADDIN" }, true);
		}

		public static List<RefCusCodeList> ReadDocFilesIntoResults(List<string> fileNames, List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex, List<string> codeTypes, bool useHardCodedColumnIndexInsteadOfPreviousWhenMissing = false)
		{
			var allWordDatas = new List<RefCusCodeList>();
			var rawDataList = new Dictionary<string, IntermediateCodeListData>();
			foreach (var fileName in fileNames)
			{
				rawDataList = fileName.EndsWith(".docx", StringComparison.InvariantCultureIgnoreCase)
					? ReadDocxFileIntoResult(codeHeaders, descriptionHeaders, codeRegex, rawDataList, fileName)
					: ReadDocFileIntoResult(codeHeaders, descriptionHeaders, codeRegex, rawDataList, fileName, useHardCodedColumnIndexInsteadOfPreviousWhenMissing);
			}

			foreach (var recordToAdd in rawDataList.Values)
			{
				var converted = codeTypes.Select(t => ConvertReadCodeDataToReCusCodeList(recordToAdd, t));
				if (converted != null)
				{
					allWordDatas.AddRange(converted.Where(c => c != null));
				}
			}
			return allWordDatas;
		}

		static Dictionary<string, IntermediateCodeListData> ReadDocxFileIntoResult(List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex, Dictionary<string, IntermediateCodeListData> rawDataList, string fileName)
		{
			var descriptionLanguageCode = fileName.ToUpperInvariant().Contains("FR") ? "FR" : "NL";
			using (var file = File.OpenRead(fileName))
			{
				System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
				var document = new XWPFDocument(file);
				var codes = new List<string>();
				var descriptions = new List<string>();
				var startDate = GetStartDateFromSupplementTable(document);

				foreach (var table in document.Tables)
				{
					var headers = table.Rows[0].GetTableCells().Select(c => c.GetText().Trim()).ToList();
					var codeHeaderIndex = headers.FindIndex(h => codeHeaders.Contains(h));
					var descriptionHeaderIndex = headers.FindIndex(h => descriptionHeaders.Contains(h));

					var rows = table.Rows;
					rows.RemoveAt(0);
					if (codeHeaderIndex != -1 && descriptionHeaderIndex != -1)
					{
						foreach (var row in rows)
						{
							var code = row.GetCell(codeHeaderIndex).GetText().Trim();
							var matchingCodeRegex = codeRegex?.FirstOrDefault(r => Regex.IsMatch(code, r));
							var description = row.GetCell(descriptionHeaderIndex).GetText().Trim();
							description = description.Length >= 2000 ? string.Concat(description.Substring(0, 1990), "...") : description;

							if (((codeRegex == null || codeRegex.Count == 0) || !string.IsNullOrEmpty(matchingCodeRegex)) && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
							{
								code = string.IsNullOrEmpty(matchingCodeRegex) ? code : Regex.Match(code, matchingCodeRegex).Value;

								IntermediateCodeListData data;
								if (!rawDataList.TryGetValue(code, out data))
								{
									data = new IntermediateCodeListData
									{
										Code = code,
										StartDate = startDate,
										MultilingualDescriptions = new Dictionary<string, string>()
									};
									rawDataList.Add(data.Code, data);
								}
								if (!data.MultilingualDescriptions.ContainsKey(descriptionLanguageCode))
								{
									data.MultilingualDescriptions.Add(descriptionLanguageCode, description);
								}
							}
						}
					}
				}
			}

			return rawDataList;
		}

		static Dictionary<string, IntermediateCodeListData> ReadDocFileIntoResult(List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex, Dictionary<string, IntermediateCodeListData> rawDataList, string fileName, bool useHardCodedColumnIndexInsteadOfPreviousWhenMissing)
		{
			using (var file = File.OpenRead(fileName))
			{
				var document = new HWPFDocument(file);
				var extractor = new WordExtractor(document);
				var counter = 0;
				var range = document.GetRange();
				var startDate = Constants.Common.MinimumDateTime;
				Table table = null;

				var previousParagraphInTable = false;
				int? previousCodeIdx = null;
				int? previousDescIdx = null;
				for (var i = 0; i < range.NumParagraphs; i++)
				{
					counter += 1;
					var par = range.GetParagraph(i);
					var isInTable = par.IsInTable();
					if (isInTable && !previousParagraphInTable)
					{
						table = range.GetTable(par);
						if (table.Text.StartsWith("Supplementen", StringComparison.InvariantCultureIgnoreCase) || table.Text.StartsWith("Suppléments", StringComparison.InvariantCultureIgnoreCase))
						{
							startDate = GetStartDateFromVersionHistoryTable(table);
						}
						else
						{
							(rawDataList, previousCodeIdx, previousDescIdx) = processRawDataForTable(fileName, table, rawDataList, startDate, codeHeaders, descriptionHeaders, codeRegex, useHardCodedColumnIndexInsteadOfPreviousWhenMissing ? null : previousCodeIdx, useHardCodedColumnIndexInsteadOfPreviousWhenMissing ? null : previousDescIdx);
						}
					}
					previousParagraphInTable = isInTable;
				}
			}

			return rawDataList;
		}

		public static (Dictionary<string, IntermediateCodeListData>, int?, int?) processRawDataForTable(string fileName, Table table, Dictionary<string, IntermediateCodeListData> rawDataList, DateTime startDate, List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex, int? previousCodeIdx, int? previousDescIdx)
		{
			var descriptionLanguage = "NL";
			if (fileName.Contains("fr"))
			{
				descriptionLanguage = "FR";
			}

			(rawDataList, previousCodeIdx, previousDescIdx) = retrieveCodeDescFromTable(table, rawDataList, startDate, codeHeaders, descriptionHeaders, codeRegex, descriptionLanguage, previousCodeIdx, previousDescIdx);

			return (rawDataList, previousCodeIdx, previousDescIdx);
		}

#pragma warning disable CA1502
		public static (Dictionary<string, IntermediateCodeListData>, int?, int?) retrieveCodeDescFromTable(Table table, Dictionary<string, IntermediateCodeListData> rawDataList, DateTime startDate, List<string> codeHeaders, List<string> descriptionHeaders, List<string> codeRegex, string descriptionLanguageCode, int? previousCodeIdx, int? previousDescIdx)
		{
			var codeLocation = 0;
			var descLocation = 0;
			var prevRowColCount = 0;

			var orphanDescription = string.Empty;
			for (var rowIdx = 0; rowIdx < table.NumRows; rowIdx++)
			{
				var row = table.GetRow(rowIdx);
				var code = string.Empty;
				var desc = string.Empty;
				var currentRowColCount = row.NumCells();
				if (currentRowColCount < 2 || currentRowColCount < codeLocation || currentRowColCount < descLocation)
				{
					continue;
				}

				if (codeLocation == 0 || descLocation == 0 || currentRowColCount != prevRowColCount)
				{
					(codeLocation, descLocation) = DetermineCodeAndDescLocation(codeHeaders, descriptionHeaders, row, previousCodeIdx, previousDescIdx);
				}

				if (codeLocation > 0 && descLocation > 0 && row.NumCells() >= codeLocation)
				{
					var text = row.GetCell(codeLocation - 1).Text.Replace("\r\a", "");
					if (!codeHeaders.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
					{
						code = Utils.SanitizeXmlString(text);
						if (code.Contains("(S"))
						{
							code = code.Remove(code.IndexOf("(S", StringComparison.InvariantCultureIgnoreCase));
						}
						code = code.Trim();
					}

					text = row.GetCell(descLocation - 1).Text.Replace("\r\a", "");
					if (!descriptionHeaders.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
					{
						desc = Utils.SanitizeXmlString(text).Replace("\r", "");
					}

					if (string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc))
					{
						orphanDescription = desc;
					}
					else if (!string.IsNullOrEmpty(code) && string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(orphanDescription))
					{
						desc = orphanDescription;
						orphanDescription = string.Empty;
					}

					if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc))
					{
						desc = desc.Trim();
						if (desc.Length >= 2000)
						{
							desc = string.Concat(desc.Substring(0, 1990), "...");
						}

						var codes = code.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);

						foreach (var cd in codes)
						{
							code = cd.Replace(" ", "").Replace("°", "").Trim();
							code = getValidCodeByRegex(code, codeRegex);

							if (code == null)
							{
								continue;
							}
							IntermediateCodeListData data = null;

							if (!rawDataList.TryGetValue(code, out data))
							{
								data = new IntermediateCodeListData();
								data.Code = code;
								data.StartDate = startDate;
								data.MultilingualDescriptions = new Dictionary<string, string>();
								rawDataList.Add(data.Code, data);
							}
							if (!data.MultilingualDescriptions.ContainsKey(descriptionLanguageCode))
							{
								data.MultilingualDescriptions.Add(descriptionLanguageCode, desc);
							}
						}
						orphanDescription = string.Empty;
					}
				}

				prevRowColCount = currentRowColCount;
			}
			return (rawDataList, codeLocation == 0 ? previousCodeIdx : codeLocation, descLocation == 0 ? previousDescIdx : descLocation);
		}
#pragma warning restore CA1502

		static (int, int) DetermineCodeAndDescLocation(List<string> codeHeaders, List<string> descriptionHeaders, TableRow row, int? previousCodeIdx, int? previousDescIdx)
		{
			var codeIdx = 0;
			var descIdx = 0;
			var hasHeader = false;

			for (var colIdx = 0; colIdx < row.NumCells(); colIdx++)
			{
				var cell = row.GetCell(colIdx);
				if (cell != null && codeHeaders.Contains(cell.Text.Clean().Trim()))
				{
					codeIdx = colIdx + 1;
					hasHeader = true;
				}
			}
			if (hasHeader)
			{
				for (var colIdx = 0; colIdx < row.NumCells(); colIdx++)
				{
					var cell = row.GetCell(colIdx);
					if (cell != null && descriptionHeaders.Contains(cell.Text.Clean().Trim()))
					{
						descIdx = colIdx + 1;
					}
				}
			}
			else
			{
				codeIdx = previousCodeIdx != null ? (int)previousCodeIdx : row.NumCells();
				descIdx = previousDescIdx != null ? (int)previousDescIdx : codeIdx - 1;
			}
			return (codeIdx, descIdx);
		}

		public static DateTime GetStartDateFromVersionHistoryTable(Table table)
		{
			var returnDate = Constants.Common.MinimumDateTime;
			for (var rowIdx = 0; rowIdx < table.NumRows; rowIdx++)
			{
				var row = table.GetRow(rowIdx);
				for (var colIdx = 0; colIdx < row.NumCells(); colIdx++)
				{
					var cell = row.GetCell(colIdx);
					if (cell != null)
					{
						var text = cell.Text.Replace("\a", "");
						text = text.Replace("\r", "");
						var historyDate = GetDateTimeFromString(text);
						if (historyDate.CompareTo(returnDate) > 0)
						{
							returnDate = historyDate;
						}
					}
				}
			}
			return returnDate;
		}

		public static DateTime GetStartDateFromSupplementTable(XWPFDocument document)
		{
			var supplementTables = document.Tables.Where(t => t.Rows[0].GetCell(0).GetText().Trim().ToUpperInvariant() == "SUPPLEMENTEN");
			return supplementTables.Any() ? supplementTables.Select(t => GetStartDateFromVersionHistoryTable(t)).Max() : Constants.Common.MinimumDateTime;
		}

		public static DateTime GetStartDateFromVersionHistoryTable(XWPFTable table)
		{
			return table.Rows.SelectMany(t => t.GetTableCells().Select(c => GetDateTimeFromString(c.GetText()))).Max();
		}

		public static DateTime GetDateTimeFromString(string text)
		{
			DateTime historyDate;
			DateTime.TryParse(text.Trim(), CultureInfo.CreateSpecificCulture("nl-NL"), DateTimeStyles.None, out historyDate);
			return historyDate;
		}

		public static string getValidCodeByRegex(string code, List<string> codeRegex)
		{
			if (codeRegex != null)
			{
				foreach (var regex in codeRegex)
				{
					if (Regex.IsMatch(code, regex))
					{
						return Regex.Match(code, regex).Value;
					}
				}
				return null;
			}

			return code;
		}

		public static RefCusCodeList ConvertReadCodeDataToReCusCodeList(IntermediateCodeListData recordToAdd, string codeType)
		{
			var defaultLanguage = "NL";
			var dutchDescription = "";
			var descDictionary = recordToAdd.MultilingualDescriptions ?? new Dictionary<string, string>();
			if (descDictionary.ContainsKey(defaultLanguage))
			{
				dutchDescription = descDictionary[defaultLanguage];
			}

			RefCusCodeList result = null;
			if (!string.IsNullOrEmpty(dutchDescription))
			{
				var refCusCodeListLanguages = new List<RefCusCodeListLanguage>();
				foreach (var descriptionItem in descDictionary.Where(x => x.Key != defaultLanguage))
				{
					refCusCodeListLanguages.Add(new RefCusCodeListLanguage()
					{
						ZXA_ZX6_NKLanguage = descriptionItem.Key,
						ZXA_Description = descriptionItem.Value
					});
				}

				return new RefCusCodeList
				{
					ZZD_ZZK_NKCodeType = codeType,
					ZZD_Code = recordToAdd.Code,
					ZZD_StartDate = recordToAdd.StartDate,
					ZZD_Description = dutchDescription,
					RefCusCodeListLanguages = refCusCodeListLanguages.ToArray()
				};
			}
			return result;
		}
	}
}
