using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	internal class CustomsFacilities : Dictionary<(string, short), Description>
	{
		internal CustomsFacilities(DownloadResult customsFacilitiesDownload)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using (var inputStream = new MemoryStream(customsFacilitiesDownload.Content))
			{
				var workbook = new XSSFWorkbook(inputStream);

				if (workbook.NumberOfSheets != 1)
				{
					throw new InvalidOperationException($"Customs Facilities workbook does not contain only one sheet. Number of sheets: {workbook.NumberOfSheets}");
				}

				if (!(workbook.GetSheetAt(0) is XSSFSheet sheet))
				{
					throw new InvalidOperationException("Customs Facilities sheet is not a worksheet");
				}

				var titleRow = sheet.GetRow(1);
				var Tn8NummColumn = titleRow.FindColumn("Tn8 Numm", "TN8 Nr");
				var ZcoCodeColumn = titleRow.FindColumn("Zco Code");
				var ZbgDBezeichnungColumn = titleRow.FindColumn("Zbg D Bezeichnung", "ZEL Txt D");
				var ZbgFBezeichnungColumn = titleRow.FindColumn("Zbg F Bezeichnung", "ZEL Txt F");
				var ZbgIBezeichnungColumn = titleRow.FindColumn("Zbg I Bezeichnung", "ZEL Txt I");
				var ZbgEBezeichnungColumn = titleRow.FindColumn("Zbg E Bezeichnung", "ZEL Txt E");

				for (int rowIndex = 2; ; rowIndex++)
				{
					var row = sheet.GetRow(rowIndex);
					if (row == null)
					{
						break;
					}

					var tn8Numm = row.GetCell(Tn8NummColumn)?.GetStringValueSafe();
					if (string.IsNullOrEmpty(tn8Numm))
					{
						break;
					}

					var zcoCode = (short)row.GetCell(ZcoCodeColumn).GetIntValueSafe();

					Add((tn8Numm, zcoCode), new Description()
					{
						TextD = row.GetCell(ZbgDBezeichnungColumn).GetStringValueSafe(),
						TextF = row.GetCell(ZbgFBezeichnungColumn).GetStringValueSafe(),
						TextI = row.GetCell(ZbgIBezeichnungColumn).GetStringValueSafe(),
						TextE = row.GetCell(ZbgEBezeichnungColumn).GetStringValueSafe(),
					});
				}
			}
		}

		internal bool TryGetValue(string commodityCode, short customsFavourCode, out Description description)
		{
			return TryGetValue((commodityCode, customsFavourCode), out description);
		}
	}
}
