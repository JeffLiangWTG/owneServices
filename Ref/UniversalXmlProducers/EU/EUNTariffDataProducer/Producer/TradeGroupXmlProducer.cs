using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TradeGroupXmlProducer : XmlProducer<RefCusTradeGroup>
	{
		public TradeGroupXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}
		public override string FilePath => ApplicationConfig.TradeGroupUXmlFile;
		public override string DataSource => ApplicationConfig.TradeGroupDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var tradeGroupConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_StartDate, false);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_EndDate, false, new DateTime(2079, 6, 6, 23, 59, 0));
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_Description, false);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, "EUN");
			tradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);

			var tradeGroupCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_Description, false);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, false);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_EndDate, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountryConfiguration);

			return writerConfiguration;
		}
	}
}
