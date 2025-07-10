using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public class ProductCodeConfiguration : ConfigurationBase
	{
		public ProductCodeConfiguration(string airlineId) : base(airlineId)
		{
		}

		protected override string DataSource => "Airline Product Codes";

		protected override IXmlWriterConfiguration GetWriterConfiguration()
		{
			var airlineProductCodeConfiguration = new EntityTypeConfiguration<RefAirlineProductCode>(true);
			airlineProductCodeConfiguration.IncludeColumn(x => x.RAR_Code, true);
			airlineProductCodeConfiguration.IncludeColumn(x => x.RAR_AirlineID, true);
			airlineProductCodeConfiguration.IncludeColumn(x => x.RAR_Description);
			airlineProductCodeConfiguration.IncludeColumn(x => x.RefAirlineProductCodeCommodityCodePivots);

			var airlineProductCodeCommodityCodePivotsConfiguration = new EntityTypeConfiguration<RefAirlineProductCodeCommodityCodePivot>(true);
			airlineProductCodeCommodityCodePivotsConfiguration.IncludeColumn(x => x.RPC_AirlineID, true);
			airlineProductCodeCommodityCodePivotsConfiguration.IncludeColumn(x => x.RPC_RAC_NKCode, true);
			airlineProductCodeCommodityCodePivotsConfiguration.IncludeColumn(x => x.RPC_RAC_NKAirlineID, true);

			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(airlineProductCodeConfiguration);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(airlineProductCodeCommodityCodePivotsConfiguration);

			return xmlWriterConfiguration;
		}
	}
}
