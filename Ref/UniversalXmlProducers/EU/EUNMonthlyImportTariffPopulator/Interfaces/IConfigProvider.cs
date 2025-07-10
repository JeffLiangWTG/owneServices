namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface IConfigProvider
	{
		string XMLDistributionUrl { get; }
		string ImportTariffUXmlFile { get; }
		string[] Filter_GoodsNomenclatures { get; }
		string[] Filter_MeasureTypes { get; }
		string[] Filter_GeographicalAreaIds { get; }
		string MeasureTypeLink_alternative { get; }
		string MeasureLink_alternative { get; }
		string MeasureConditionCode_alternative { get; }
		string GoodsNomenclatureLink_alternative { get; }
		string DeclarableGoodsNomenclatureLink_alternative { get; }
		string PublicationTime_alternative { get; }
		string RegulationLink_alternative { get; }
		string DownloadFolder_alternative { get; }
		bool HasFilters { get; }
	}
}
