using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgSupplierBuyerLinkTolerance
	{
		ZGuid PK { get; }
		ZByte OLT_EarlyShipmentLimitDays { get; }
		ZByte OLT_LateShipmentLimitDays { get; }
		ZDecimal OLT_UnderQuantityPercentageLimit { get; }
		ZDecimal OLT_OverQuantityPercentageLimit { get; }
	}
}
