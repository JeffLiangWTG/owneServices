using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming.Values;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class PermitInformation : Dictionary<string, Description>
	{
		public PermitInformation(DownloadResult permitInformationDownload)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using (var inputStream = new MemoryStream(permitInformationDownload.Content))
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
				var bewCodeColumn = headerRow.FindColumn("BEW Code");
				var sciColumn = headerRow.FindColumn("STE Nr", "STI Nr", "Sci Numm", "Sce Numm");
				var permitRemarksDColumn = headerRow.FindColumn("BWS Txt D", "Bws D Bezeichnung");
				var permitRemarksFColumn = headerRow.FindColumn("BWS Txt F", "Bws F Bezeichnung");
				var permitRemarksIColumn = headerRow.FindColumn("BWS Txt I", "Bws I Bezeichnung");
				var permitRemarksEColumn = headerRow.FindColumn("BWS Txt E", "Bws E Bezeichnung");
				var bslTypColumn = headerRow.FindColumn("BSL Typ");
				var bewTolColumn = headerRow.FindColumn("TOL Wert", "BEW Tol Toleranz");
				var toleranceRemarksDColumn = headerRow.FindColumn("TBS Txt D", "Tbs D Bezeichnung");
				var toleranceRemarksFColumn = headerRow.FindColumn("TBS Txt F", "Tbs F Bezeichnung");
				var toleranceRemarksIColumn = headerRow.FindColumn("TBS Txt I", "Tbs I Bezeichnung");
				var toleranceRemarksEColumn = headerRow.FindColumn("TBS Txt E", "Tbs E Bezeichnung");

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
					var authorityCode = row.GetCell(bewCodeColumn).GetIntValueSafe();

					var bewTolValue = row.GetCell(bewTolColumn).GetDecimalValueSafe();
					int toleranceCode;
					switch (bewTolValue)
					{
						case 0m:
							toleranceCode = 1;
							break;
						case 2.5m:
							toleranceCode = 2;
							break;
						case 20m:
							toleranceCode = 3;
							break;
						default:
							toleranceCode = 0;
							break;
					}
					if (toleranceCode == 0)
					{
						continue;
					}

					var includeExclude = row.GetCell(bslTypColumn).GetStringValueSafe();
					if (includeExclude != "E")
					{
						continue;
					}

					var key = CreateKey(tariffNumber, statisticalCode, authorityCode, toleranceCode);
					if (!ContainsKey(key))
					{
						Add(key, new Description()
						{
							TextD = GetRemarks(row, permitRemarksDColumn, toleranceRemarksDColumn),
							TextF = GetRemarks(row, permitRemarksFColumn, toleranceRemarksFColumn),
							TextI = GetRemarks(row, permitRemarksIColumn, toleranceRemarksIColumn),
							TextE = GetRemarks(row, permitRemarksEColumn, toleranceRemarksEColumn),
						});
					}
				}
			}
		}

		static string GetRemarks(IRow row, int permitRemarksColumn, int toleranceRemarksColumn)
		{
			var permitRemarks = row.GetCell(permitRemarksColumn)?.GetStringValueSafe();
			var toleranceRemarks = row.GetCell(toleranceRemarksColumn)?.GetStringValueSafe();
			var remarks = new StringBuilder();

			if (!string.IsNullOrEmpty(permitRemarks))
			{
				remarks.Append(permitRemarks);
			}

			if (!string.IsNullOrEmpty(toleranceRemarks))
			{
				if (remarks.Length > 0)
				{
					remarks.Append(", ");
				}
				remarks.Append(toleranceRemarks);
			}

			return remarks.ToString();
		}

		internal static string CreateKey(string commodityCode, int statisticalCode, int authorityCode, int toleranceCode)
		{
			return $"{commodityCode}|{statisticalCode}|{authorityCode}|{toleranceCode}";
		}

		const int StartRow = 2;
	}
}
