using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class ExportUnionTariffLoader
	{
		public static IEnumerable<CodeDescriptionPair> LoadData(string path)
		{
			var records = new List<CodeDescriptionPair>();

			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				var code = xls.GetStringFromCell(row, 1).Trim();
				var description = xls.GetStringFromCell(row, 2).Trim();

				records.Add(new CodeDescriptionPair { Code = code, Description = description });
			}

			var duplicateRecordsWithoutLargestDescription = records.GroupBy(record => record.Code).Where(grouping => grouping.Count() > 1)
				.SelectMany(grouping => grouping.OrderByDescending(pair => pair.Description.Sum(character => char.GetNumericValue(character))).Skip(1));

			return records.Except(duplicateRecordsWithoutLargestDescription);
		}
	}
}
