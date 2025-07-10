using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackNAFTATariff
	{
		ZString NAFTACountryImportEntry { get; }
		ZDate NAFTACountryImportEntryDate { get; }
		ZString NAFTACountryTariffNumber { get; }
		ZDecimal NAFTACountryDutyRate { get; }
		ZDecimal NAFTACountryImportDuty { get; }
		ZDecimal EquivalentUSDollarAmountOfNAFTACountryDuty { get; }
	}
}
