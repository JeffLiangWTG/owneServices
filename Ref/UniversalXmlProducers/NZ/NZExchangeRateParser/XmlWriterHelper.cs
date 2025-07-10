using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefExchangeRateZZConfiguration()
		{
			var exchangeRateZZConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "NZ");
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateZZConfiguration);

			return writerConfiguration;
		}
	}
}
