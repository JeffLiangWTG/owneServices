using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class CommonDataXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.CommonData);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var commonDataConfiguration = new EntityTypeConfiguration<UNDGCommonData>(true);
			commonDataConfiguration.IncludeColumn(x => x.DC_Type, true);
			commonDataConfiguration.IncludeColumn(x => x.DC_Descriptor);
			commonDataConfiguration.IncludeColumnWithConstantValue(x => x.DC_Language, true, "EN");
			commonDataConfiguration.IncludeColumn(x => x.DC_Index, true);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(commonDataConfiguration);

			return xmlWriterConfig;
		}
	}
}
