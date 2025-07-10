using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class ETradeExemptionCodesLoader
	{
		public static IEnumerable<ETradeExemptionCodes> LoadData(string path)
		{
			var records = new List<ETradeExemptionCodes>();

			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				records.Add(new ETradeExemptionCodes
				{
					ExemptionCode = xls.GetStringFromCell(row, 1).Trim(),
					ExemptionDescEnglish = xls.GetStringFromCell(row, 2).Trim(),
					DutyPercent = xls.GetStringFromCell(row, 3).Trim(),
					DutyFormula = xls.GetStringFromCell(row, 4).Trim(),
					ExemptionDescTurkish = xls.GetStringFromCell(row, 5).Trim(),
					RateCode = xls.GetStringFromCell(row, 7).Trim(),
					RateType = xls.GetStringFromCell(row, 6).Trim(),
					TradeGroup = xls.GetStringFromCell(row, 8).Trim(),
					StartDate = Common.DateTimeHelper.ParseDate(xls.GetStringFromCell(row, 9).Trim()).Value,
					EndDate = Common.DateTimeHelper.ParseDate(xls.GetStringFromCell(row, 10).Trim()).Value
				});
			}
			return records;
		}
	}
}
