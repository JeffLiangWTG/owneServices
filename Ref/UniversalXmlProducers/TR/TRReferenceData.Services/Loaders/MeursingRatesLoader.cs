using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class MeursingRatesLoader
	{
		public static IEnumerable<MeursingRates> LoadData(string path)
		{
			var records = new List<MeursingRates>();

			using (WordprocessingDocument doc = WordprocessingDocument.Open(path, false))
			{
				Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();

				var rows = table.Elements<TableRow>().Skip(1);

				foreach (var row in rows)
				{
					var cells = row.Elements<TableCell>().ToList();
					if (cells.Count > 0)
					{
						string codeNumber = cells[0].InnerText?.Trim();
						string t1 = cells.Count > 1 ? cells[1].InnerText?.Trim() : string.Empty;
						string t2 = cells.Count > 2 ? cells[2].InnerText?.Trim() : string.Empty;

						if (!string.IsNullOrEmpty(codeNumber) && codeNumber.All(char.IsDigit))
						{
							records.Add(new MeursingRates
							{
								CodeNumber = codeNumber,
								T1 = t1,
								T2 = t2
							});
						}
					}
				}
			}
			return records;
		}

	}
}
