using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	internal class DailyConfigProvider : IDailyConfigProvider
	{
		#region Daily

		public string DailyXMLDistributionUrl => ApplicationConfig.DailyXMLDistributionUrl;

		public string DailyImportTariffUXmlFile => ApplicationConfig.DailyImportTariffUXmlFile;
		public string DailyPublicationTime_alternative => ApplicationConfig.DailyPublicationTime_alternative;
		public string DownloadFolder_alternative => ApplicationConfig.DownloadFolder_alternative;

		#endregion

		#region Monthly

		public string MonthlyXMLDistributionUrl => ApplicationConfig.MonthlyXMLDistributionUrl;

		public string[] Filter_GoodsNomenclatures => ApplicationConfig.Filter_GoodsNomenclatures.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

		public string[] Filter_MeasureTypes => ApplicationConfig.Filter_MeasureTypes.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

		public string[] Filter_GeographicalAreaIds => ApplicationConfig.Filter_GeographicalAreaIds.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

		public string MeasureTypeLink_alternative => ApplicationConfig.MeasureTypeLink_alternative;

		public string MeasureLink_alternative => ApplicationConfig.MeasureLink_alternative;

		public string MeasureConditionCode_alternative => ApplicationConfig.MeasureConditionCode_alternative;

		public string DeclarableGoodsNomenclatureLink_alternative => ApplicationConfig.DeclarableGoodsNomenclatureLink_alternative;

		public string PublicationTime_alternative => ApplicationConfig.RegulationLink_alternative;

		public string RegulationLink_alternative => ApplicationConfig.PublicationTime_alternative;

		#endregion
	}
}
