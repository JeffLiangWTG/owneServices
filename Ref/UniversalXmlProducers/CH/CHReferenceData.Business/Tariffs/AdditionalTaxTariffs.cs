using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class AdditionalTaxTariffs : Dictionary<(string, string), AdditionalTaxTariff>
	{
		internal AdditionalTaxTariffs(DownloadResult keyStructureAdditionalTaxesDownload)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using (var inputStream = new MemoryStream(keyStructureAdditionalTaxesDownload.Content))
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
				var tn8NummColumn = headerRow.FindColumn("Tn8 Numm", "TN8 Nr");
				var sciNummColumn = headerRow.FindColumn("Sci Numm", "STI Nr");
				var zusOptColumn = headerRow.FindColumn("Zus Opt");
				var zusGrppflichtColumn = headerRow.FindColumn("Zus Grppflicht", "ZUS Gruppenpflicht");
				var enaCodeColumn = headerRow.FindColumn("Ena Code");
				var zagZuschlColumn = headerRow.FindColumn("Zag Zuschl");
				var zanAnsColumn = headerRow.FindColumn("Zan Ans");
				var zanMinColumn = headerRow.FindColumn("Zan Min");
				var zanMaxColumn = headerRow.FindColumn("Zan Max");
				var bmcCodeColumn = headerRow.FindColumn("Bmc Code", "ZAG BGL Id");
				var bmcFaktorColumn = headerRow.FindColumn("Bmc Faktor", "ZAG Faktor");
				var zslTypColumn = headerRow.FindColumn("Zsl Typ");
				var ldgNummColumn = headerRow.FindColumn("Ldg Numm", "LDG Nr");

				for (int rowIndex = FirstRow; ; rowIndex++)
				{
					var row = sheet.GetRow(rowIndex);
					if (row == null || row.GetCell(enaCodeColumn).GetStringValueSafe().Length == 0)
					{
						break;
					}

					var type = row.GetCell(enaCodeColumn).GetIntValueSafe().ToString("000", CultureInfo.InvariantCulture);
					var key = row.GetCell(zagZuschlColumn).GetStringValueSafe();
					if (!TryGetValue((type, key), out var additionalTaxTariff))
					{
						additionalTaxTariff = new AdditionalTaxTariff()
						{
							assessmentCode = row.GetCell(bmcCodeColumn).GetIntValueSafe(),
							factor = row.GetCell(bmcFaktorColumn).GetIntValueSafe(),
							rate = row.GetCell(zanAnsColumn).GetDecimalValueSafe(),
							rateMin = row.GetCell(zanMinColumn).GetDecimalValueSafe(),
							rateMax = row.GetCell(zanMaxColumn).GetDecimalValueSafe(),
						};
						Add((type, key), additionalTaxTariff);
					}

					var commodityCode = row.GetCell(tn8NummColumn).GetStringValueSafe();
					var statisticalCode = row.GetCell(sciNummColumn).GetIntValueSafe();
					if (!additionalTaxTariff.relationships.TryGetValue((commodityCode, statisticalCode), out var relationship))
					{
						relationship = new AdditionalTaxRelationship()
						{
							commodityCode = commodityCode,
							statisticalCode = statisticalCode,
							isOptional = row.GetCell(zusOptColumn).GetStringValueSafe() == "J",
							groupControl = row.GetCell(zusGrppflichtColumn).GetIntValueSafe(),
						};
						additionalTaxTariff.relationships.Add((commodityCode, statisticalCode), relationship);
					}

					var tradeGroup = row.GetCell(ldgNummColumn).GetIntValueSafe();
					if (row.GetCell(zslTypColumn).GetStringValueSafe() == "E")
					{
						relationship.tradeGroup = tradeGroup;
					}
					else
					{
						relationship.excludedTradeGroups.Add(tradeGroup);
						additionalTaxTariff.hasExclusions = true;
					}
				}
			}
		}

		internal bool TryGetValue(string type, string key, out AdditionalTaxTariff additionaltax)
		{
			return TryGetValue((type, key), out additionaltax);
		}

		const int FirstRow = 2;
	}
}
