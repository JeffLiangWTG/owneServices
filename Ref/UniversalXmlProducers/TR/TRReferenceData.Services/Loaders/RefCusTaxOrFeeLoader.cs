using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class RefCusTaxOrFeeLoader
	{
		public static IEnumerable<RefCusTaxOrFee> LoadData(string path)
		{
			var records = new List<RefCusTaxOrFee>();

			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				records.Add(new RefCusTaxOrFee
				{
					ZZF_Code = xls.GetStringFromCell(row, 1).Trim(),
					ZZF_Description = xls.GetStringFromCell(row, 2).Trim(),
					ZZF_Value = decimal.Parse(xls.GetStringFromCell(row, 4).Trim(), style, CultureInfo.InvariantCulture),
					ZZF_StartDate = (DateTime)ParseDate(xls.GetStringFromCell(row, 5).Trim()),
					ZZF_EndDate = (DateTime)ParseDate(xls.GetStringFromCell(row, 6).Trim()),
					ZZF_ZZZ_NKDataGrouping = xls.GetStringFromCell(row, 7).Trim(),
					ZZF_ZX0_NKTaxOrFeeType = xls.GetStringFromCell(row, 11).Trim(),
					RefCusTaxOrFeeLanguages = new[]
					{
						new RefCusTaxOrFeeLanguage
						{
							ZXU_Description = xls.GetStringFromCell(row, 3).Trim(),
							ZXU_ZX6_NKLanguage = Constants.CountryCodeTR
						}
					}
				});
			}
			return records;
		}

		public static DateTime? ParseDate(string dateAsString)
		{
			DateTime? date = null;
			if (!string.IsNullOrEmpty(dateAsString))
			{
				if (DateTime.TryParse(dateAsString, out DateTime dateAsDate))
				{
					date = dateAsDate;
				}
				else
				{
					throw new FormatException($"Parse DateTime Error: {dateAsString}");
				}
			}
			return date;
		}
	}
}
