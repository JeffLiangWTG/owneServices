using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public class CommodityCodeConfiguration : ConfigurationBase
	{
		public CommodityCodeConfiguration(string airlineId) : base(airlineId)
		{
		}

		protected override string DataSource => "Airline specific Commodity Codes";

		protected override IXmlWriterConfiguration GetWriterConfiguration()
		{
			var airlineCommodityCodeConfiguration = new EntityTypeConfiguration<RefAirlineCommodityCode>(true);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_Code, true);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_Description, false);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_AirlineID, true);
			airlineCommodityCodeConfiguration.IncludeColumn(x => x.RAC_SpecialHandlingCodes, false);
			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(airlineCommodityCodeConfiguration);

			return xmlWriterConfiguration;
		}
	}
}
