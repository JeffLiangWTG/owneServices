using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer
{
	public static class XmlWriterConfiguration
	{
		public static IXmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var configuration = GetWriterConfiguration();
			var xmlWriter = new XmlWriter(configuration);
			xmlWriter.SetDataSource(Constants.AirlineCommodityCodesDataSource);
			xmlWriter.SetPublicationTime(publicationTime);

			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var airlineCommodityCodeConfiguration = new EntityTypeConfiguration<RefAirlineCommodityCode>(true);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_Code, true);
			airlineCommodityCodeConfiguration.IncludeColumnWithDefaultValue(x => x.RAC_AirlineID, true, string.Empty);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_Description);

			var xmlWriterConfiguration = new RefDbRepo.Common.UniversalXmlWriter.XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(airlineCommodityCodeConfiguration);

			return xmlWriterConfiguration;
		}
	}
}
