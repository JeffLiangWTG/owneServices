using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobPaymentBasis
	{
		ZString AdapterID { get; }
		ZString AdapterType { get; }
		ZString ChargeableDescription { get; }
		ZDecimal MinRate { get; }
		ZDecimal MaxRate { get; }
		ZDecimal FlatRate { get; }
		ZDecimal PerUnitRate { get; }
		ZDecimal ChargeableAmount { get; }
		ZString ChargeableUnit { get; }
		ZString ChargeableUnitType { get; }
		ZString RateUnit { get; }
		ZString RateUnitType { get; }
		ZString RateCurrency { get; }
		ZString RateCurrencyDescription { get; }
		ZString RateReference { get; }
	}
}
