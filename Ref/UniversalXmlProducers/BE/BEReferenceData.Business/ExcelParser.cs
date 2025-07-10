using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class ExcelParser
	{
		static Dictionary<string, int> months = new Dictionary<string, int>
		{
			["JAN"] = 1,
			["FEB"] = 2,
			["MAR"] = 3,
			["APR"] = 4,
			["MAY"] = 5,
			["JUN"] = 6,
			["JUL"] = 7,
			["AUG"] = 8,
			["SEP"] = 9,
			["OCT"] = 10,
			["NOV"] = 11,
			["DEC"] = 12
		};

		public static List<RefExchangeRateZZ> ReadExchangeRateXlsIntoResults(string fileName, DateTime inputStartDate, DateTime inputEndDate)
		{
			using (var reader = File.OpenRead(fileName))
			{
				var sheetIndex = 1;
				var startingRow = 3;
				var startingMonthColumn = 4;
				var lastRow = int.MaxValue;
				var checkExtraMonthField = true;

				var allExcelDatas = new List<RefExchangeRateZZ>();
				var xlsFile = new XlsFile(reader, false);
				sheetIndex = xlsFile.GetSheetIndex(inputStartDate.Year.ToString(CultureInfo.InvariantCulture), false);

				if (sheetIndex == -1)
				{
					var findSheet = 1;
					while (findSheet <= xlsFile.SheetCount)
					{
						var sheetName = xlsFile.GetSheetName(findSheet);
						if (sheetName != null && inputStartDate.Year.ToString(CultureInfo.InvariantCulture) == sheetName)
						{
							sheetIndex = findSheet;
							break;
						}

						findSheet++;
					}
				}

				if (sheetIndex == -1)
				{
					return allExcelDatas;
				}

				xlsFile.SetSheetSelected(sheetIndex, true);
				xlsFile.ActiveSheet = sheetIndex;

				var rowCount = xlsFile.GetRowCount(sheetIndex);
				var monthIndex = inputStartDate.Month;

				// If a value is given in B2, then we are probably parsing the unlisted currencies => no headers in 2nd row like in listed currencies xlsx
				if (xlsFile.GetCellValue(2, 2) != null)
				{
					checkExtraMonthField = false;
					startingRow = 2;
				}

				for (var monthColId = startingMonthColumn; monthColId <= xlsFile.ColCountOnlyData; monthColId++)
				{
					if (xlsFile.GetCellValue(1, monthColId) != null)
					{
						var xlsMonth = xlsFile.GetCellValue(1, monthColId)?.ToString();
						months.TryGetValue(xlsMonth, out monthIndex);
					}
					if (inputStartDate.Month == monthIndex)
					{
						var start = inputStartDate;
						var end = inputEndDate;

						if (checkExtraMonthField)
						{
							var extraMonthField = xlsFile.GetCellValue(2, monthColId)?.ToString();
							if (extraMonthField != null)
							{
								var conv = DateTime.FromOADate(int.Parse(extraMonthField, CultureInfo.InvariantCulture)).Date;
								start = new DateTime(inputStartDate.Year, inputStartDate.Month, conv.Day, 0, 0, 0);
								extraMonthField = xlsFile.GetCellValue(2, monthColId + 1)?.ToString();
								conv = DateTime.FromOADate(int.Parse(extraMonthField, CultureInfo.InvariantCulture)).Date;
								end = new DateTime(inputEndDate.Year, conv.Month, conv.Day, 23, 59, 59);
								end = end.AddDays(-1);
								if (end.Month != inputEndDate.Month)
								{
									end = inputEndDate;
								}
							}
						}

						for (var rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
						{
							var rate = xlsFile.GetCellValue(rowId, monthColId)?.ToString();

							if (!string.IsNullOrEmpty(rate))
							{
								var excelData = new RefExchangeRateZZ
								{
									ZZN_StartDate = start,
									ZZN_EndDate = end,
									ZZN_Rate = ConvertNumberToCurrentLocale(rate),
									ZZN_RX_NKExCurrency = xlsFile.GetCellValue(rowId, 3)?.ToString(),
									ZZN_RN_NKCountry = xlsFile.GetCellValue(rowId, 2)?.ToString()
								};

								allExcelDatas.Add(excelData);
							}
						}
					}
				}

				return allExcelDatas;
			}
		}

		public static List<RefCusCodeList> ReadLocationCodesXlsIntoResults(string fileName, string sheetName)
		{
			using (var reader = File.OpenRead(fileName))
			{
				var sheetIndex = 1;
				var startingRow = 2;
				var lastRow = int.MaxValue;

				var allExcelDatas = new List<RefCusCodeList>();
				var xlsFile = new XlsFile(reader, false);

				var findSheet = 1;
				while (findSheet <= xlsFile.SheetCount)
				{
					if (sheetName != null && xlsFile.GetSheetName(findSheet) == sheetName)
					{
						sheetIndex = findSheet;
						break;
					}

					findSheet++;
				}

				if (sheetIndex == -1)
				{
					return allExcelDatas;
				}

				xlsFile.SetSheetSelected(sheetIndex, true);
				xlsFile.ActiveSheet = sheetIndex;

				var rowCount = xlsFile.GetRowCount(sheetIndex);

				for (var rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
				{
					var locationCode = xlsFile.GetCellValue(rowId, 1)?.ToString();
					var name = xlsFile.GetCellValue(rowId, 2)?.ToString();
					var street = xlsFile.GetCellValue(rowId, 3)?.ToString();
					var postal = xlsFile.GetCellValue(rowId, 4)?.ToString();
					var city = xlsFile.GetCellValue(rowId, 5)?.ToString();
					var type = xlsFile.GetCellValue(rowId, 6)?.ToString();
					var subtype = xlsFile.GetCellValue(rowId, 7)?.ToString();

					if (!string.IsNullOrEmpty(locationCode))
					{
						var excelData = new RefCusCodeList
						{
							ZZD_Code = locationCode,
							ZZD_Description = string.Concat(name, " ", street, " ", postal, " ", city).Trim(),
							ZZD_ZZK_NKCodeType = Constants.ZZRefCusCodeList.AdditionalCode,
							ZZD_ZZZ_NKDataGrouping = Constants.Common.LocalCountryCode
						};

						if (string.IsNullOrEmpty(excelData.ZZD_Description))
						{
							excelData.ZZD_Description = "N/A";
						}

						// get all descriptions for all other available languages
						var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
						if (!string.IsNullOrEmpty(street))
						{
							refCusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = "Street", ZZE_Value = street });
						}
						if (!string.IsNullOrEmpty(city))
						{
							refCusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = "City", ZZE_Value = city });
						}
						if (!string.IsNullOrEmpty(postal))
						{
							refCusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = "PostCode", ZZE_Value = postal });
						}
						if (!string.IsNullOrEmpty(type))
						{
							refCusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = "Type", ZZE_Value = type });
						}
						if (!string.IsNullOrEmpty(subtype))
						{
							refCusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = "SubType", ZZE_Value = subtype });
						}

						if (refCusCodeListAttributes.Count > 0)
						{
							excelData.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
						}

						allExcelDatas.Add(excelData);
					}
				}
				return allExcelDatas;
			}
		}

		static decimal ConvertNumberToCurrentLocale(string input)
		{
			input = input.Replace(" ", string.Empty);
			string separator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			switch (separator)
			{
				case ".":
					input = input.Replace(",", ".");
					break;
				case ",":
					input = input.Replace(".", ",");
					break;
			}
			return decimal.Parse(input, Thread.CurrentThread.CurrentCulture);
		}
	}
}
