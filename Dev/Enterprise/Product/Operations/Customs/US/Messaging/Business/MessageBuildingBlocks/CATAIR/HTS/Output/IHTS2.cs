using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS2
	{
		ZString TariffNumber { get; }
		ZDecimal Column1RateAdValorem { get; }
		ZDecimal Column1RateOther { get; }
		ZDecimal Column2RateSpecific { get; }
		ZDecimal Column2RateAdValorem { get; }
		ZDecimal Column2RateOther { get; }
		ZString CountervailingDutyFlag { get; }
		ZString AdditionalTariffNumberIndicator { get; }
		ZString MiscellaneousPermitLicenseIndicator { get; }
	}
}
