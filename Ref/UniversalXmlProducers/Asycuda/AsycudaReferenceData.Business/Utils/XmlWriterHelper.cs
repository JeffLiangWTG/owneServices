using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Business;

public static class XmlWriterHelper
{
	public static XmlWriterConfiguration GetExchangeRateWriterConfiguration(DateTime startDate, DateTime endDate, string datagroup)
	{
		var writerConfiguration = new XmlWriterConfiguration();

		var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
		exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
		exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
		exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, endDate);
		exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
		exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
		exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, datagroup);

		writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
		return writerConfiguration;
	}
}
