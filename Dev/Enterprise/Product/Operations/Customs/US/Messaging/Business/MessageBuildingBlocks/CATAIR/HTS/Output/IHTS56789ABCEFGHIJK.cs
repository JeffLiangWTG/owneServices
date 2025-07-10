using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS56789ABCEFGHIJK
	{
		ZString DutyElement { get; }
		ZString TariffNumber { get; }
		ZString InternationalOrganizationForStandardizationISOCountryCode { get; }
		ZDecimal SpecificSpecialRate { get; }
		ZDecimal AdValoremSpecialRate { get; }
		ZDecimal OtherSpecialRate { get; }
		ZString TaxFeeClassCode { get; }
		ZString TaxFeeComputationCode { get; }
		ZString TaxFeeFlag { get; }
		ZDecimal TaxFeeSpecificRate { get; }
		ZDecimal TaxFeeAdValorem { get; }
	}
}
