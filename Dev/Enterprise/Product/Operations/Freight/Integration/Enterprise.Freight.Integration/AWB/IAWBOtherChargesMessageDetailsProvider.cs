using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBOtherChargesMessageDetailsProvider
	{
		ZString ChargeCode { get; }
		ZString EntitlementCode { get; }
		ZDecimal Amount { get; }
	}
}
