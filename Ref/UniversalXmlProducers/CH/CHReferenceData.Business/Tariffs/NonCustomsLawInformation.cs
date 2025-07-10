using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public class NonCustomsLawInformation : Dictionary<string, Description>
	{
		internal NonCustomsLawInformation(DownloadResult nonCustomsLawInformationDownload)
		{
			using (var inputStream = new MemoryStream(nonCustomsLawInformationDownload.Content))
			{
				var workbook = new XSSFWorkbook(inputStream);

				if (workbook.NumberOfSheets != 1)
				{
					throw new InvalidOperationException($"Key information workbook does not contain only one sheet. Number of sheets: {workbook.NumberOfSheets}");
				}

				if (!(workbook.GetSheetAt(0) is XSSFSheet sheet))
				{
					throw new InvalidOperationException("Key information sheet is not a worksheet");
				}

				var headerRow = sheet.GetRow(StartRow - 1);
				var tn8NummColumn = headerRow.FindColumn("TN8 Nr", "Tn8 Numm");
				var sciColumn = headerRow.FindColumn("STI Nr", "STE Nr", "Sci Numm", "Sce Numm");
				var nzmCodeColumn = headerRow.FindColumn("NZM Code", "Nze Artc");
				var nzsTxtDColumn = headerRow.FindColumn("NZS Txt D", "Nzs D Bezeichnung");
				var nzsTxtFColumn = headerRow.FindColumn("NZS Txt F", "Nzs F Bezeichnung");
				var nzsTxtIColumn = headerRow.FindColumn("NZS Txt I", "Nzs I Bezeichnung");
				var nzsTxtEColumn = headerRow.FindColumn("NZS Txt E", "Nzs E Bezeichnung");

				for (int rowIndex = StartRow; ; rowIndex++)
				{
					var row = sheet.GetRow(rowIndex);
					if (row == null)
					{
						break;
					}

					var tariffNumber = row.GetCell(tn8NummColumn)?.GetStringValueSafe();
					if (string.IsNullOrEmpty(tariffNumber))
					{
						break;
					}

					var statisticalCode = row.GetCell(sciColumn).GetIntValueSafe();
					var nonCustomsLawCode = row.GetCell(nzmCodeColumn).GetIntValueSafe();

					var key = CreateKey(tariffNumber, statisticalCode, nonCustomsLawCode);
					if (!ContainsKey(key))
					{
						Add(key, new Description()
						{
							TextD = GetRemarks(row, nzsTxtDColumn),
							TextF = GetRemarks(row, nzsTxtFColumn),
							TextI = GetRemarks(row, nzsTxtIColumn),
							TextE = GetRemarks(row, nzsTxtEColumn),
						});
					}
				}
			}
		}

		static string GetRemarks(IRow row, int permitRemarksColumn)
		{
			var permitRemark = row.GetCell(permitRemarksColumn)?.GetStringValueSafe().Trim();
			var remarks = new StringBuilder();

			if (!string.IsNullOrEmpty(permitRemark))
			{
				remarks.Append(permitRemark);
			}

			return remarks.ToString().TrimEnd();
		}

		internal static string CreateKey(string commodityCode, int statisticalCode, int nonCustomsLawCode)
		{
			return $"{commodityCode}|{statisticalCode}|{nonCustomsLawCode}";
		}

		const int StartRow = 2;
	}
}
