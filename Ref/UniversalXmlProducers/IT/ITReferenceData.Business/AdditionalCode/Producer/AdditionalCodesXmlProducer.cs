using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public class AdditionalCodesXmlProducer
	{
		public AdditionalCodesXmlProducer(IXmlProducerOption option)
		{
			this.option = Argument.NotNull(option, nameof(option));
			dataSourceName = Argument.NotNullOrEmpty(option.DataSourceName, nameof(option.DataSourceName));
			fileName = Argument.NotNullOrEmpty(option.FileName, nameof(option.FileName));
		}

		readonly IXmlProducerOption option;
		readonly string dataSourceName;
		readonly string fileName;

		public void ExportToXml(IEnumerable<RefCusCodeList> additionalCodeCollection, string filePath)
		{
			var xmlWriter = InitializeXmlWriter();
			foreach (var additionalCode in additionalCodeCollection)
			{
				xmlWriter.PopulateData(additionalCode);
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
			var refCusCodeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			refCusCodeListConfiguration.IncludeColumn(x => x.ZZD_StartDate);
			refCusCodeListConfiguration.IncludeColumn(x => x.ZZD_EndDate);
			refCusCodeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.RefCusCodeListCodeTypes.AdditionalCode);
			refCusCodeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Business.Constants.RefDataGroupings.Italy);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusCodeListConfiguration);
			return xmlWriterConfig;
		}
	}
}
