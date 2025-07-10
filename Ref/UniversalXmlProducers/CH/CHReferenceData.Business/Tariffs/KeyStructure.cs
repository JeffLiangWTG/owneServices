using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	abstract class KeyStructure : Dictionary<string, Description>
	{
		internal KeyStructure(DownloadResult keyStructureDownload, bool isPrefaceDictionary)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using (var inputStream = new MemoryStream(keyStructureDownload.Content))
			{
				var workbook = new XSSFWorkbook(inputStream);

				if (workbook.NumberOfSheets != 1)
				{
					throw new InvalidOperationException($"Key structure workbook does not contain only one sheet. Number of sheets: {workbook.NumberOfSheets}");
				}

				if (!(workbook.GetSheetAt(0) is XSSFSheet sheet))
				{
					throw new InvalidOperationException("Key structure sheet is not a worksheet");
				}

				var headerRow = sheet.GetRow(FirstRow - 1);
				var tn8NummColumn = headerRow.FindColumn("Tn8 Numm");
				var typColumn = headerRow.FindColumn("Typ");
				var schluesselColumn = headerRow.FindColumn("Schlüssel");
				var levelColumn = headerRow.FindColumn("Einrueck");
				textDColumn = headerRow.FindColumn("Text D");
				textFColumn = headerRow.FindColumn("Text F");
				textIColumn = headerRow.FindColumn("Text I");
				textEColumn = headerRow.FindColumn("Text E");

				var vlsKeys = new List<int>() { 0 };

				for (var rowIndex = FirstRow; ; rowIndex++)
				{
					var row = sheet.GetRow(rowIndex);
					if (row == null)
					{
						break;
					}

					var typ = row.GetCell(typColumn)?.GetStringValueSafe();
					if (string.IsNullOrEmpty(typ))
					{
						break;
					}

					var level = row.GetCell(levelColumn).GetIntValueSafe();

					if (typ == VTyp)
					{
						vlsKeys.SetDynamicListItem(level, rowIndex);
						if (isPrefaceDictionary)
						{
							AddEntry(rowIndex.ToString(CultureInfo.InvariantCulture), row, vlsKeys[level - 1]);
						}
					}
					else if (typ == STyp && !isPrefaceDictionary)
					{
						var numm = row.GetCell(tn8NummColumn).GetStringValueSafe();
						var schluessel = row.GetCell(schluesselColumn).GetStringValueSafe();
						AddEntry(numm + KeySeparator + schluessel, row, vlsKeys[level - 1]);
					}
				}
			}
		}

		void AddEntry(string key, IRow row, int vlsKey)
		{
			Add(key, new Description()
			{
				TextD = row.GetCell(textDColumn).GetStringValueSafe(),
				TextF = row.GetCell(textFColumn).GetStringValueSafe(),
				TextI = row.GetCell(textIColumn).GetStringValueSafe(),
				TextE = row.GetCell(textEColumn).GetStringValueSafe(),
				VKey = vlsKey == 0 ? string.Empty : vlsKey.ToString(CultureInfo.InvariantCulture),
			});
		}

		protected abstract string VTyp { get; }

		protected abstract string STyp { get; }

		internal const string KeySeparator = " ";

		const int FirstRow = 2;
		readonly int textDColumn;
		readonly int textFColumn;
		readonly int textIColumn;
		readonly int textEColumn;
	}
}
