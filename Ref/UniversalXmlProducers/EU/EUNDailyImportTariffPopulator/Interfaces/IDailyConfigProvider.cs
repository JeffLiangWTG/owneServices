namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDailyConfigProvider
	{
		#region Daily configuration

		string DailyPublicationTime_alternative { get; }
		string DailyXMLDistributionUrl { get; }
		string DailyImportTariffUXmlFile { get; }
		string DownloadFolder_alternative { get; }

		#endregion

		#region Monthly configuration
		string MonthlyXMLDistributionUrl { get; }
		string[] Filter_GoodsNomenclatures { get; }
		string[] Filter_MeasureTypes { get; }
		string[] Filter_GeographicalAreaIds { get; }
		string MeasureTypeLink_alternative { get; }
		string MeasureLink_alternative { get; }
		string MeasureConditionCode_alternative { get; }
		string DeclarableGoodsNomenclatureLink_alternative { get; }
		string PublicationTime_alternative { get; }
		string RegulationLink_alternative { get; }
		#endregion
	}
}
