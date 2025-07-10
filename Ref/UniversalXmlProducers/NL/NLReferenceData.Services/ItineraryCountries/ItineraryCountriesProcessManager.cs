namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class ItineraryCountriesProcessManager : CommonExcelProcessManagerAbstract<ItineraryCountriesData>
	{
		public ItineraryCountriesProcessManager(IDataBuilder<ItineraryCountriesData> dataBuilder) : base(dataBuilder, new ItineraryCountriesExcelParser())
		{
		}

		protected override string ResourceContent => "CargoWise.RefDbRepo.NLReferenceData.Services.ItineraryCountries.Ref.ItineraryCountries.xlsx";
		protected override string ExcelFileName => "ItineraryCountries.xlsx";
		protected override string EntityName => "Itinerary Countries";
	}
}
