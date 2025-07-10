using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class NomenclatureTariffLoader
	{
		public static Chapter LoadData(string fileFullName, string chapterCode)
		{
			var result = new Chapter { Code = chapterCode };
			var records = new List<RawNomenclatureTariff>();

			XlsFile xls = new XlsFile(fileFullName);
			xls.ActiveSheet = 1;
			var isHeader = true;
			for (int row = 1; row <= xls.RowCount; row++)
			{
				var colA = xls.GetStringFromCell(row, 1).Trim();
				var colB = xls.GetStringFromCell(row, 2);
				if (isHeader)
				{
					var isSubchapter = string.IsNullOrEmpty(colA) && !string.IsNullOrWhiteSpace(colB);
					var isValidCode = colA.ToString(CultureInfo.InvariantCulture).StartsWith(chapterCode, StringComparison.Ordinal);
					if (isSubchapter || isValidCode)
					{
						isHeader = false;
					}
					else
					{
						continue;
					}
				}

				if (string.IsNullOrWhiteSpace(colB))
				{
					continue;
				}

				var colC = xls.GetStringFromCell(row, 3).Trim();
				var colD = xls.GetStringFromCell(row, 4).Trim();

				var format = xls.GetFormat(xls.GetCellFormat(row, 2));

				records.Add(new RawNomenclatureTariff { SeqNum = row, Code = colA, Description = colB, UOM = colC, DutyRate = colD, DescriptionFormat = format });
			}

			result.Records = records.OrderBy(x => x.SeqNum);
			return result;
		}

		public static IOrderedEnumerable<RawNomenclatureTariff> LoadData(string fileFullName)
		{
			var records = new List<RawNomenclatureTariff>();

			XlsFile xls = new XlsFile(fileFullName);
			xls.ActiveSheet = 1;
			for (int row = 2; row <= xls.RowCount; row++)
			{
				var colA = xls.GetStringFromCell(row, 1).Trim().ToString(CultureInfo.InvariantCulture).Trim((char)0xA0);
				var colB = xls.GetStringFromCell(row, 2);

				if (string.IsNullOrWhiteSpace(colB))
				{
					continue;
				}

				var colC = xls.GetStringFromCell(row, 3).Trim();
				var colD = xls.GetStringFromCell(row, 4).Trim();

				var format = xls.GetFormat(xls.GetCellFormat(row, 2));

				records.Add(new RawNomenclatureTariff { SeqNum = row, Code = colA, Description = colB, UOM = colC, DutyRate = colD, DescriptionFormat = format });
			}

			return records.OrderBy(x => x.SeqNum);
		}

		public static DateTime LoadPublicationTime(string fileFullName)
		{
			XlsFile xls = new XlsFile(fileFullName);
			xls.ActiveSheet = 1;
			_ = DateTime.TryParse(xls.GetStringFromCell(1, 2).Trim(), out var result);
			return result;
		}
	}
}
