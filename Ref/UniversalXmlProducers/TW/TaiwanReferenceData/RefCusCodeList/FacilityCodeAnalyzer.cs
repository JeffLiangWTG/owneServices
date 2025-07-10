using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.TaiwanReferenceData.RefCusCodeListUpdateInfo;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class FacilityCodeAnalyzer
	{
		static readonly DateTime defaultStartDate = new DateTime(1900, 01, 01, 0, 0, 0);
		static readonly DateTime defaultEndDate = new DateTime(2079, 06, 06, 23, 59, 0);

		static readonly Regex portRegex = new Regex("^[A-Z0-9]{3}$");
		static readonly Regex codeRegex = new Regex("^[A-Z0-9]{8}$");
		static readonly Regex customsRegex = new Regex("^[A-Z]{2}$");

		const int PortCodeIndex = 0;
		const int LocationCodeIndex = PortCodeIndex + 1;
		const int FullCodeIndex = 2;

		List<string> errorMessages = new List<string>();

		public void PopulateCusCodeListsForFacilityCode(IEnumerable<byte[]> downloadedDatas, List<RefCusCodeList> RefCusCodeLists)
		{
			foreach (var data in downloadedDatas)
			{
				var dataByCode = new Dictionary<string, (string Description, string CustomsCode, string Note)>();
				var dataTables = OdfHelper.ExtractTableToDataTables(data);
				foreach (var dataTable in dataTables)
				{
					var dataRows = dataTable.Rows.Cast<DataRow>().ToArray();
					if (FacilityCodeIsInTwoColumns(dataRows, out var firstDataRow))
					{
						ReadDataToPairsForCodeInTwoColumns(dataByCode, dataRows, firstDataRow);
					}
					else if (FacilityCodeIsInOneColumn(dataRows, out firstDataRow))
					{
						ReadDataToPairsForCodeInOneColumn(dataByCode, dataRows, firstDataRow);
					}
				}

				foreach (var facilityInfo in dataByCode)
				{
					var (errorMessage, startDate, endDate) = GetDateForActivities(facilityInfo);

					if (!string.IsNullOrEmpty(errorMessage))
					{
						errorMessages.Add($"{facilityInfo.Key}: Analyzation Error (Description: {facilityInfo.Value.Description}, Note: {facilityInfo.Value.Note}): {errorMessage}");
					}

					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = facilityInfo.Key,
						ZZD_Description = GetDescriptionWithoutWhiteSpace(facilityInfo.Value.Description).SafeSubstring(0, 2000),
						ZZD_EndDate = endDate,
						ZZD_StartDate = startDate,
						RefCusCodeListAttributes = new RefCusCodeListAttribute[]
						{
							new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.CUSTOMSOFFICE, ZZE_Value = facilityInfo.Value.CustomsCode }
						},
					};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}

			if (errorMessages.Count > 0)
			{
				Console.OutputEncoding = Encoding.UTF8;
				Console.Error.WriteLine($"[ERROR]{string.Join("\r\n", errorMessages)}");
			}
		}

		static string GetValueAsString(object obj) => obj?.ToString() ?? string.Empty;

		static int GetCustomsCodeIndex(DataRow firstDataRow)
		{
			var result = -1;
			var columnCount = firstDataRow.Table.Columns.Count;

			for (var i = 0; i < columnCount; i++)
			{
				if (customsRegex.IsMatch(GetValueAsString(firstDataRow[i])))
				{
					result = i;
					break;
				}
			}

			return result;
		}

		static int GetNoteIndex(DataRow[] dataRows)
		{
			var result = -1;
			var columnCount = dataRows[0].Table.Columns.Count;
			foreach (var dataRow in dataRows)
			{
				var thisRowColumn = columnCount - 1;
				for (; thisRowColumn > result && thisRowColumn >= 0; thisRowColumn--)
				{
					if (!string.IsNullOrEmpty(GetValueAsString(dataRow[thisRowColumn])))
					{
						break;
					}
				}
				result = Math.Max(result, thisRowColumn);
			}
			return result;
		}

		static bool FacilityCodeIsInTwoColumns(DataRow[] dataRows, out DataRow firstDataRow)
		{
			firstDataRow = dataRows.FirstOrDefault(row => portRegex.IsMatch(GetValueAsString(row[PortCodeIndex])));
			return firstDataRow != null;
		}

		static bool FacilityCodeIsInOneColumn(DataRow[] dataRows, out DataRow firstDataRow)
		{
			firstDataRow = dataRows.FirstOrDefault(row => codeRegex.IsMatch(GetValueAsString(row[FullCodeIndex])));
			return firstDataRow != null;
		}

		static void ReadDataToPairsForCodeInTwoColumns(Dictionary<string, (string Description, string CustomsCode, string Note)> dataByCode, DataRow[] dataRows, DataRow firstDataRow)
		{
			var customsCodeIndex = GetCustomsCodeIndex(firstDataRow);
			if (customsCodeIndex >= 0)
			{
				const int descriptionIndex = 2;
				var noteIndex = GetNoteIndex(dataRows);
				var rowCount = dataRows.Length;

				for (var i = 0; i < rowCount; i++)
				{
					var dataRow = dataRows[i];

					var portCode = GetValueAsString(dataRow[PortCodeIndex]);
					if (portRegex.IsMatch(portCode))
					{
						var locationCode = GetValueAsString(dataRow[LocationCodeIndex]);
						var description = GetValueAsString(dataRow[descriptionIndex]);
						var note = GetValueAsString(dataRow[noteIndex]);
						var code = RemoveWhiteSpace(portCode + locationCode);

						if ((i + 1) < rowCount)
						{
							var nextRow = dataRows[i + 1];
							if (string.IsNullOrEmpty(GetValueAsString(nextRow[PortCodeIndex]))
								&& !string.IsNullOrEmpty(GetValueAsString(nextRow[descriptionIndex])))
							{
								description = GetValueAsString(nextRow[descriptionIndex]);
								note = GetValueAsString(nextRow[noteIndex]);
								i++;
							}
						}

						if (dataByCode.TryGetValue(code, out var entry))
						{
							entry.Description += $", {description}";
							entry.Note += $"\n{note}";
							dataByCode[code] = entry;
						}
						else
						{
							var customsCode = GetValueAsString(dataRow[customsCodeIndex]);
							dataByCode[code] = (description, customsCode, note);
						}
					}
				}
			}
		}

		static void ReadDataToPairsForCodeInOneColumn(Dictionary<string, (string Description, string CustomsCode, string Note)> dataByCode, DataRow[] dataRows, DataRow firstDataRow)
		{
			var customsCodeIndex = GetCustomsCodeIndex(firstDataRow);
			if (customsCodeIndex >= 0)
			{
				var descriptionIndex = 3;
				var noteIndex = GetNoteIndex(dataRows);

				foreach (var dataRow in dataRows)
				{
					var code = RemoveWhiteSpace(GetValueAsString(dataRow[FullCodeIndex]));

					if (codeRegex.IsMatch(code))
					{
						var description = GetValueAsString(dataRow[descriptionIndex]);
						var customsCode = GetValueAsString(dataRow[customsCodeIndex]);
						var note = GetValueAsString(dataRow[noteIndex]);
						dataByCode[code] = (description, customsCode, note);
					}
				}
			}
		}

		static string RemoveWhiteSpace(string input) => input.Replace(" ", "").Replace("\t", "");

		static string GetDescriptionWithoutWhiteSpace(string desc)
		{
			string res = desc.Trim();
			res = res.Replace("\r", "");
			res = res.Replace('\n', ' ');
			res = res.Replace('\t', ' ');
			return res;
		}

		#region Notes Analyzation

		enum ActivityType
		{
			NotActivity,
			New,
			Update,
			Delete,
			Unknow
		}

		static Regex keywords_Exclude = new Regex("(?<!編)號|倉|棟|(交註銷)|(交刪除)");
		static string[] keywords_Unknow = { };
		static string[] keywords_New = { "新增", "設立" };
		static string[] keywords_Update = { "變更", "更新", "更正", "更名", "擴增", "修正", "修訂" };
		static string[] keywords_Delete = { "註銷", "撤銷", "廢止", "刪除", "停用", "取代", "暫停", "暫行停業" };

		static readonly Regex dateRegex = new Regex("自?[0-9]{2,3}(\\.|年)[0-9]{1,2}(\\.|月)[0-9]{1,2}日?");

		static (List<string> activityList, List<string> dateList) SplitStringIntoActivitiesAndDates(string noteLine)
		{
			var activity = new List<string>();
			var dateList = new List<string>();
			var matchResult = dateRegex.Matches(noteLine);

			var lastStart = 0;
			foreach (Match m in matchResult)
			{
				var dateStringBuilder = new StringBuilder(m.Value);
				dateStringBuilder.Replace("年", ".").Replace("月", ".").Replace("日", ".").Replace("自", "");
				dateList.Add(dateStringBuilder.ToString());

				if (m.Index > lastStart)
				{
					activity.Add(noteLine.Substring(lastStart, m.Index - lastStart));
				}
				lastStart = Math.Max(lastStart, m.Index + m.Value.Length);
			}
			if (lastStart < noteLine.Length)
			{
				activity.Add(noteLine.Substring(lastStart));
			}

			return (activity, dateList);
		}

		static bool ContainsAny(string str, IEnumerable<string> values) => values.Any(v => str.Contains(v));

		static (ActivityType activityType, string errorMessage) AnalyzeActivity(string activity)
		{
			var activityType = ActivityType.Unknow;
			var errorMessage = string.Empty;

			if (keywords_Exclude.IsMatch(activity))
			{
				activityType = ActivityType.NotActivity;
			}
			else if (ContainsAny(activity, keywords_Unknow))
			{
				activityType = ActivityType.Unknow;
				errorMessage = $"Unknown activity: {activity}";
			}
			else
			{
				var isNew = false;
				var isUpdate = false;
				var isDelete = false;
				var containsIn = 0;

				if (ContainsAny(activity, keywords_New))
				{
					containsIn++;
					isNew = true;
					activityType = ActivityType.New;
				}
				if (ContainsAny(activity, keywords_Update))
				{
					containsIn++;
					isUpdate = true;
					activityType = ActivityType.Update;
				}
				if (ContainsAny(activity, keywords_Delete))
				{
					containsIn++;
					isDelete = true;
					activityType = ActivityType.Delete;
				}

				if (containsIn > 1)
				{
					activityType = ActivityType.Unknow;
					errorMessage = "This activity belongs to mutiple types:";
					if (isNew)
					{
						errorMessage += " [New]";
					}
					if (isUpdate)
					{
						errorMessage += " [Update]";
					}
					if (isDelete)
					{
						errorMessage += " [Delete]";
					}
				}
			}

			return (activityType, errorMessage);
		}

		static (string errorMessage, DateTime startDate, DateTime endDate) GetDateForActivities(KeyValuePair<string, (string Description, string CustomsCode, string Note)> facilityInfo)
		{
			var newestDate = defaultStartDate;
			var startDate = defaultStartDate;
			var endDate = defaultEndDate;
			var errorMessage = string.Empty;
			var facilityInfoNote = facilityInfo.Value.Note;

			/*
			 * Original Text
			 * 94.05.03擴增監管編號變更
			 * 111.05.17
			 * 
			 * Formatted Text
			 * 94.05.03擴增
			 * 監管編號變更111.05.17
			 */
			facilityInfoNote = Regex.Replace(facilityInfoNote, @"^([0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2}){1}(擴增監管編號變更){1}\n{1}([0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2}){1}$", "$1擴增\n監管編號變更$3");

			/*
			 * Original Text
			 * 新增鴻明\n註銷陽明\n103.07.15
			 * Formatted Text
			 * 新增鴻明103.07.15
			 */
			facilityInfoNote = Regex.Replace(facilityInfoNote, @"^(?<NoteText1>[^\n]+)\n(?<NoteText2>[^\n]+)\n(?<Date>[0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2})$", "${NoteText1}${Date}");

			/*
			 * Original Text
			 * 103.07.15新增鴻明註銷陽明 OR 103.07.15新增鴻明註銷陽明\n含MCC拆併作業專區
			 * Formatted Text
			 * 新增鴻明103.07.15
			 */
			facilityInfoNote = Regex.Replace(facilityInfoNote, @"^(?<Date>[0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2})(?<NoteText1>新增[^0-9]+)(?<NoteText2>註銷[^0-9]+)", "${NoteText1}${Date}");

			/*
			 * Original Text
			 * 監管編號變更\n111.05.17
			 * Formatted Text
			 * 監管編號變更111.05.17
			 */
			facilityInfoNote = Regex.Replace(facilityInfoNote, @"^(?<NoteText>[^\n]+)\n(?<Date>[0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2})$", "${NoteText}${Date}");

			var notes = facilityInfoNote.Split('\n');
			foreach (var note in notes)
			{
				/*
				 * Original Text
				 * 新增95.10.17(原台南縣)
				 * Formatted Text
				 * 新增95.10.17
				 */
				var formattedNote = Regex.Replace(note.Trim(), @"(\([^\)]*\))$", "");

				formattedNote = formattedNote.Replace("註銷更正", "更正").Replace("變更註銷", "註銷");

				/*
				 * Original Text
				 * BA註銷更正管轄關別為：BT（95.09.15）
				 * Formatted Text
				 * 95.09.15BA註銷更正管轄關別為：BT
				 */
				formattedNote = Regex.Replace(formattedNote, @"^(?<NoteText>\D+)（(?<Date>(?<=（)([0-9]{2,3}.[0-9]{1,2}.[0-9]{1,2})(?=）$))", "${Date}${NoteText}");

				var (noteDate, noteErrorMessage, noteStartDate, noteEndDate) = GetDateForActivitiesInOneNoteLine(formattedNote);
				if (noteDate >= newestDate)
				{
					startDate = noteStartDate;
					endDate = noteEndDate;
					errorMessage = noteErrorMessage;
					newestDate = noteDate;
				}
			}

			return (errorMessage, startDate, endDate);
		}

		static (DateTime noteDate, string errorMessage, DateTime startDate, DateTime endDate) GetDateForActivitiesInOneNoteLine(string note)
		{
			var startDate = defaultStartDate;
			var endDate = defaultEndDate;
			var noteDate = defaultStartDate;
			var errorMessage = string.Empty;

			var (activityList, dateList) = SplitStringIntoActivitiesAndDates(note);

			if (activityList.Count == 0)
			{
				if (dateList.Count > 0)
				{
					var errorMessageBuilder = new StringBuilder("Cannot find activity for date");
					foreach (var dateStr in dateList)
					{
						errorMessageBuilder.Append(" " + dateStr);
					}
					errorMessage = errorMessageBuilder.ToString();
				}
			}
			else if (activityList.Count == 1)
			{
				if (dateList.Count > 1)
				{
					var errorMessageBuilder = new StringBuilder("Cannot find activity for date");
					for (int i = 1; i < dateList.Count; i++)
					{
						errorMessageBuilder.Append(" " + dateList[i]);
					}
					errorMessage = errorMessageBuilder.ToString();
				}
				else
				{
					var (activityType, noteErrorMessage) = AnalyzeActivity(activityList.First());
					if (!string.IsNullOrEmpty(noteErrorMessage))
					{
						errorMessage = $"Unknown activity {activityList.First()}";
					}
					else if (activityType != ActivityType.NotActivity)
					{
						if (dateList.Count == 1 && Utility.TryConvertTaiwanDateStringToDateTime(dateList.First(), out var date))
						{
							if (activityType == ActivityType.Delete)
							{
								endDate = date;
							}
							else
							{
								startDate = date;
							}
							noteDate = date;
						}
						else if (activityType == ActivityType.Delete)
						{
							endDate = startDate;
						}
					}
				}
			}
			else
			{
				if (activityList.Count != dateList.Count)
				{
					errorMessage = $"The number of dateList and activities are not equal: find {dateList.Count} dateList(s) but {activityList.Count} activity(ies)";
				}
				else
				{
					var newestDateID = -1;
					var newestDate = defaultStartDate;
					for (var i = 0; i < dateList.Count; i++)
					{
						if (Utility.TryConvertTaiwanDateStringToDateTime(dateList[i], out var date) && date > newestDate)
						{
							newestDate = date;
							newestDateID = i;
						}
					}

					var (activityType, noteErrorMessage) = AnalyzeActivity(activityList[newestDateID]);
					if (!string.IsNullOrEmpty(noteErrorMessage))
					{
						errorMessage = $"Unknown activity {activityList.First()}";
					}
					else if (activityType != ActivityType.NotActivity)
					{
						if (activityType == ActivityType.Delete)
						{
							endDate = newestDate;
						}
						else
						{
							startDate = newestDate;
						}
						noteDate = newestDate;
					}

				}
			}

			return (noteDate, errorMessage, startDate, endDate);
		}

		#endregion
	}
}
