using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgPartRelation
	{
		ZGuid PK { get; }
		ZString CategoryCode { get; }
		ZBool OU_UseExpiryDate { get; }
		ZBool OU_UseSerialNumber { get; }
		ZBool OU_IsSerialNumberReleaseCaptured { get; }
		ZString OU_PickMode { get; }
		ZDecimal OU_UnitPrice { get; }
		ZString OU_RX_NKUnitPriceCurrency { get; }
	}
}
