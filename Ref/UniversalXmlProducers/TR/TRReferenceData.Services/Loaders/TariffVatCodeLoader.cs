using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class TariffVatCodeLoader
	{
		public static IEnumerable<TariffVatCode> Load(string filePath)
		{
			var records = new List<TariffVatCode>();

			var xls = new XlsFile(filePath);
			xls.ActiveSheet = 1;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				var tariffCode = xls.GetStringFromCell(row, 1).Trim().Replace(".", "");
				var vat1 = xls.GetStringFromCell(row, 2).Trim().Replace("KDV", "KD").Value.ToUpper(CultureInfo.InvariantCulture);
				var vat2 = xls.GetStringFromCell(row, 3).Trim().Replace("KDV", "KD").Value.ToUpper(CultureInfo.InvariantCulture);
				var vat3 = xls.GetStringFromCell(row, 4).Trim().Replace("KDV", "KD").Value.ToUpper(CultureInfo.InvariantCulture);

				var codeDescriptions = new Dictionary<string, string>
				{
					{ "KD1", "KDV %1" },
					{ "KD10", "KDV %10" },
					{ "KD20", "KDV %20" },
					{ "MUAF", "MUAF" },
				};

				if (!string.IsNullOrEmpty(vat1))
				{
					records.Add(new TariffVatCode { TariffCode = tariffCode, VatCode = vat1, Description = codeDescriptions[vat1] });
				}

				if (!string.IsNullOrEmpty(vat2))
				{
					records.Add(new TariffVatCode { TariffCode = tariffCode, VatCode = vat2, Description = codeDescriptions[vat2] });
				}

				if (!string.IsNullOrEmpty(vat3))
				{
					records.Add(new TariffVatCode { TariffCode = tariffCode, VatCode = vat3, Description = codeDescriptions[vat3] });
				}
			}

			return records;
		}
	}
}
