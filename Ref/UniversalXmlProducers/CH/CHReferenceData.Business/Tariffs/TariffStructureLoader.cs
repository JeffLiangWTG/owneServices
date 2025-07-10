using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class TariffStructureLoader
	{
		internal static void Load(DownloadResult tariffStructureDownload, Action<string, Description> consumer, bool includeAll)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using (var inputStream = new MemoryStream(tariffStructureDownload.Content))
			{
				var workbook = new XSSFWorkbook(inputStream);

				if (workbook.NumberOfSheets != 1)
				{
					throw new InvalidOperationException($"Tariff structure workbook does not contain only one sheet. Number of sheets: {workbook.NumberOfSheets}");
				}

				if (!(workbook.GetSheetAt(0) is XSSFSheet sheet))
				{
					throw new InvalidOperationException("Tariff structure sheet is not a worksheet");
				}

				var keys = new List<int>();
				var level = 0;
				var tukOffset = 0;

				for (int rowIndex = 1; ; rowIndex++)
				{
					var row = sheet.GetRow(rowIndex);
					if (row == null)
					{
						break;
					}

					var typ = row.GetCell(TypColumn)?.GetStringValueSafe();
					if (string.IsNullOrEmpty(typ))
					{
						break;
					}

					var previousLevel = level;
					var numm = row.GetCell(NummColumn).GetStringValueSafe();
					bool include = false;

					switch (typ)
					{
						case "TAB":
							level = 1;
							keys.SetDynamicListItem(level, ParseIntSafe(numm));
							break;
						case "TN2":
							level = 2;
							tukOffset = 0;
							keys.SetDynamicListItem(level, ParseIntSafe(numm));
							include = includeAll;
							break;
						case "TUK":
							level = 3;
							tukOffset = 1;
							keys.SetDynamicListItem(level, ParseIntSafe(numm));
							include = includeAll;
							break;
						case "TN4":
							level = 3 + tukOffset;
							keys.SetDynamicListItem(level, numm.Length >= 4 ? ParseIntSafe(numm.Substring(2, 2)) : 0);
							include = includeAll;
							break;
						case "TN6":
							level = 4 + tukOffset;
							keys.SetDynamicListItem(level, numm.Length >= 7 ? ParseIntSafe(numm.Substring(5, 2)) : 0);
							include = includeAll;
							break;
						case "VT6":
						case "VT8":
							GetLevelFromExcel();
							UpdateKeys();
							numm = "";
							include = includeAll;
							break;
						case "TN8":
							if (!GetLevelFromExcel())
							{
								level = 5 + tukOffset;
							}
							UpdateKeys();
							include = true;
							break;
					}

					if (include)
					{
						consumer(numm, new Description()
						{
							TextD = row.GetCell(TextDColumn).GetStringValueSafe(),
							TextF = row.GetCell(TextFColumn).GetStringValueSafe(),
							TextI = row.GetCell(TextIColumn).GetStringValueSafe(),
							TextE = row.GetCell(TextEColumn).GetStringValueSafe(),
							SortKey = string.Join(".", keys.Skip(1).Take(level).Select(k => k.ToString("00", CultureInfo.InvariantCulture))),
						});
					}

					bool GetLevelFromExcel()
					{
						var einrueck = row.GetCell(EinrueckColumn).GetIntValueSafe();
						if (einrueck != 0)
						{
							level = einrueck + tukOffset + 3;
							return true;
						}
						return false;
					}

					void UpdateKeys()
					{
						if (level > previousLevel)
						{
							keys.SetDynamicListItem(level, 1);
						}
						else
						{
							keys[level]++;
						}
					}
				}
			}
		}

		static int ParseIntSafe(string str) => int.TryParse(str, out int val) ? val : 0;

		const int TypColumn = 0;
		const int NummColumn = 1;
		const int EinrueckColumn = 2;
		const int TextDColumn = 4;
		const int TextFColumn = 5;
		const int TextIColumn = 6;
		const int TextEColumn = 7;
	}
}
