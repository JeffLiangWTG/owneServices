using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, Constants.RateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.CountryISO);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return xmlWriterConfig;
		}
	}
}
