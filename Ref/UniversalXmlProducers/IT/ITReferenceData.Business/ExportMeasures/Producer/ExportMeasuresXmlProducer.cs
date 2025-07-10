using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public class ExportMeasuresXmlProducer
	{
		public ExportMeasuresXmlProducer(IXmlProducerOption option)
		{
			this.option = Argument.NotNull(option, nameof(option));
			dataSourceName = Argument.NotNullOrEmpty(option.DataSourceName, nameof(option.DataSourceName));
			fileName = Argument.NotNullOrEmpty(option.FileName, nameof(option.FileName));
		}

		public void ExportToXml(IEnumerable<RefCusTariff> tariffs, string filePath)
		{
			var xmlWriter = InitializeXmlWriter();
			foreach (var tariff in tariffs)
			{
				xmlWriter.PopulateData(tariff);
			}

			xmlWriter.SaveXml(Path.Combine(filePath, fileName));
		}

		XmlWriter InitializeXmlWriter()
		{
			var xmlWriter = new XmlWriter(GetXmlWriterConfiguration());
			xmlWriter.SetDataSource(dataSourceName);
			xmlWriter.SetPublicationTime(option.PublicationDateTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffAdditionalCodes, false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.Measures.ExportTariffType);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.Measures.EuropeanUnionTradeCode);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.Measures.EuropeanUnionTradeCode);

			var refCusTariffAdditionalCodeConfiguration = new EntityTypeConfiguration<RefCusTariffAdditionalCode>(true);
			refCusTariffAdditionalCodeConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			refCusTariffAdditionalCodeConfiguration.IncludeColumn(x => x.ZY2_AdditionalCode, true);
			refCusTariffAdditionalCodeConfiguration.IncludeColumn(x => x.ZY2_Description, false);
			refCusTariffAdditionalCodeConfiguration.IncludeColumn(x => x.ZY2_ZY3_NKCategory, true);
			refCusTariffAdditionalCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZY2_ZZZ_NKDataGrouping, true, Constants.RefDataGroupings.Italy);

			var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			refCusApplicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, new DateTime(2079, 6, 6, 23, 59, 0));
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.Measures.EuropeanUnionTradeCode);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusTariffConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusTariffAdditionalCodeConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);
			return xmlWriterConfig;
		}

		readonly IXmlProducerOption option;
		readonly string dataSourceName;
		readonly string fileName;
	}
}
