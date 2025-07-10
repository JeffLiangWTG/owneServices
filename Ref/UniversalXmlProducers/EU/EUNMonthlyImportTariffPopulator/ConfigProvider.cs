using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public class ConfigProvider : IConfigProvider, IMonthlyConfigProvider
	{
		public ConfigProvider() { }

		public ConfigProvider(string xmlDistributionUrl,
			string importTariffUXmlFile,
			string[] filter_GoodsNomenclatures,
			string[] filter_MeasureTypes,
			string[] filter_GeographicalAreaIds,
			string measureTypeLink_alternative,
			string measureLink_alternative,
			string measureConditionCode_alternative,
			string declarableGoodsNomenclatureLink_alternative,
			string regulationLink_alternative,
			string publicationTime_alternative,
			string downloadFolder_alternative)
		{
			XMLDistributionUrl = xmlDistributionUrl;
			ImportTariffUXmlFile = importTariffUXmlFile;
			Filter_GoodsNomenclatures = filter_GoodsNomenclatures;
			Filter_MeasureTypes = filter_MeasureTypes;
			Filter_GeographicalAreaIds = filter_GeographicalAreaIds;
			MeasureTypeLink_alternative = measureTypeLink_alternative;
			MeasureLink_alternative = measureLink_alternative;
			MeasureConditionCode_alternative = measureConditionCode_alternative;
			DeclarableGoodsNomenclatureLink_alternative = declarableGoodsNomenclatureLink_alternative;
			RegulationLink_alternative = regulationLink_alternative;
			PublicationTime_alternative = publicationTime_alternative;
			DownloadFolder_alternative = downloadFolder_alternative;
		}

		public string XMLDistributionUrl
		{
			get
			{
				var configValue = ApplicationConfig.XMLDistributionUrl;
				if (xmlDistributionUrl == null && configValue != null)
				{
					xmlDistributionUrl = configValue;
				}
				return xmlDistributionUrl;
			}
			set
			{
				xmlDistributionUrl = value;
			}
		}
		string xmlDistributionUrl;

		public string ImportTariffUXmlFile
		{
			get
			{
				var configValue = ApplicationConfig.ImportTariffUXmlFile;
				if (importTariffUXmlFile == null && configValue != null)
				{
					importTariffUXmlFile = configValue;
				}
				return importTariffUXmlFile;
			}
			set
			{
				importTariffUXmlFile = value;
			}
		}
		string importTariffUXmlFile;

		#region Filters

		public string[] Filter_GoodsNomenclatures
		{
			get
			{
				var configValue = ApplicationConfig.Filter_GoodsNomenclatures;
				if (filter_GoodsNomenclatures == null && configValue != null)
				{
					filter_GoodsNomenclatures = configValue.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
				return filter_GoodsNomenclatures;
			}
			set
			{
				filter_GoodsNomenclatures = value;
			}
		}
		string[] filter_GoodsNomenclatures;

		public string[] Filter_MeasureTypes
		{
			get
			{
				var configValue = ApplicationConfig.Filter_MeasureTypes;
				if (filter_MeasureTypes == null && configValue != null)
				{
					filter_MeasureTypes = configValue.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
				return filter_MeasureTypes;
			}
			set
			{
				filter_MeasureTypes = value;
			}
		}
		string[] filter_MeasureTypes;

		public string[] Filter_GeographicalAreaIds
		{
			get
			{
				var configValue = ApplicationConfig.Filter_GeographicalAreaIds;
				if (filter_GeographicalAreaIds == null && configValue != null)
				{
					filter_GeographicalAreaIds = configValue.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
				return filter_GeographicalAreaIds;
			}
			set
			{
				filter_GeographicalAreaIds = value;
			}
		}
		string[] filter_GeographicalAreaIds;

		#endregion

		#region Alternatives

		public string MeasureTypeLink_alternative
		{
			get
			{
				var configValue = ApplicationConfig.MeasureTypeLink_alternative;
				if (measureTypeLink_alternative == null && configValue != null)
				{
					measureTypeLink_alternative = configValue;
				}
				return measureTypeLink_alternative;
			}
			set
			{
				measureTypeLink_alternative = value;
			}
		}
		string measureTypeLink_alternative;

		public string MeasureLink_alternative
		{
			get
			{
				var configValue = ApplicationConfig.MeasureLink_alternative;
				if (measureLink_alternative == null && configValue != null)
				{
					measureLink_alternative = configValue;
				}
				return measureLink_alternative;
			}
			set
			{
				measureLink_alternative = value;
			}
		}
		string measureLink_alternative;

		public string MeasureConditionCode_alternative
		{
			get
			{
				var configValue = ApplicationConfig.MeasureConditionCode_alternative;
				if (measureConditionCode_alternative == null && configValue != null)
				{
					measureConditionCode_alternative = configValue;
				}
				return measureConditionCode_alternative;
			}
			set
			{
				measureConditionCode_alternative = value;
			}
		}
		string measureConditionCode_alternative;

		public string GoodsNomenclatureLink_alternative
		{
			get
			{
				var configValue = ApplicationConfig.GoodsNomenclatureLink_alternative;
				if (goodsNomenclatureLink_alternative == null && configValue != null)
				{
					goodsNomenclatureLink_alternative = configValue;
				}
				return goodsNomenclatureLink_alternative;
			}
			set
			{
				goodsNomenclatureLink_alternative = value;
			}
		}
		string goodsNomenclatureLink_alternative;

		public string DeclarableGoodsNomenclatureLink_alternative
		{
			get
			{
				var configValue = ApplicationConfig.DeclarableGoodsNomenclatureLink_alternative;
				if (declarableGoodsNomenclatureLink_alternative == null && configValue != null)
				{
					declarableGoodsNomenclatureLink_alternative = configValue;
				}
				return declarableGoodsNomenclatureLink_alternative;
			}
			set
			{
				declarableGoodsNomenclatureLink_alternative = value;
			}
		}
		string declarableGoodsNomenclatureLink_alternative;

		public string RegulationLink_alternative
		{
			get
			{
				var configValue = ApplicationConfig.RegulationLink_alternative;
				if (regulationLink_alternative == null && configValue != null)
				{
					regulationLink_alternative = configValue;
				}
				return regulationLink_alternative;
			}
			set
			{
				regulationLink_alternative = value;
			}
		}
		string regulationLink_alternative;

		public string PublicationTime_alternative
		{
			get
			{
				var configValue = ApplicationConfig.PublicationTime_alternative;
				if (publicationTime_alternative == null && configValue != null)
				{
					publicationTime_alternative = configValue;
				}
				return publicationTime_alternative;
			}
			set
			{
				publicationTime_alternative = value;
			}
		}
		string publicationTime_alternative;

		public string DownloadFolder_alternative
		{
			get
			{
				var configValue = ApplicationConfig.DownloadFolder_alternative;
				if (downloadFolder_alternative == null && configValue != null)
				{
					downloadFolder_alternative = configValue;
				}
				return downloadFolder_alternative;
			}
			set
			{
				downloadFolder_alternative = value;
			}
		}
		string downloadFolder_alternative;

		#endregion

		public bool HasFilters => Filter_GoodsNomenclatures.Length > 0 || Filter_MeasureTypes.Length > 0 || Filter_GeographicalAreaIds.Length > 0;
	}
}
