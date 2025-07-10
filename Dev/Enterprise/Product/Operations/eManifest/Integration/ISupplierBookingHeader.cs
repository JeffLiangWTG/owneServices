using CargoWise.Types;

namespace Enterprise.eManifest.Integration
{
	public interface ISupplierBookingHeader
	{
		ZGuid PK { get; }
		ZGuid DH_OA_Consignor { get; set; }
		ZGuid DH_OC_ConsignorContact { get; set; }
		ZString DH_SupplierReference { get; set; }
		ZDateTime DH_SystemCreateTimeUtc { get; set; }
		ZString DH_SystemCreateUser { get; set; }
		ZDateTime DH_SystemLastEditTimeUtc { get; set; }
		ZString DH_SystemLastEditUser { get; set; }
	}
}
