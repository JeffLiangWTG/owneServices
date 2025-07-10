using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class ItineraryCountriesExcelParser : IExcelParser<ItineraryCountriesData>
	{
		public IList<ItineraryCountriesData> ReadXlsFile(IList<string> files)
		{
			var parser = new CommonExcelParser<ItineraryCountriesData>((xls, key, rowId) =>
			{
				return new ItineraryCountriesData
				{
					Code = key,
					Description = xls.GetStr(rowId, 2),
				};
			}, 1, 3, 1);

			return parser.ReadXlsFile(files);
		}
	}
}
