using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVBookingHeader : IBusiness
	{
		ZGuid PK { get; }

		ZString HVH_BookingReference { get; set; }
		ZInt HVH_ClusterKey { get; set; }
		ZDecimal HVH_GrossVolume { get; }
		ZString HVH_GrossVolumeUQ { get; set; }
		ZDecimal HVH_GrossWeight { get; }
		ZString HVH_GrossWeightUQ { get; set; }
		ZBool HVH_IsBookingConfirmed { get; set; }
		ZBool HVH_IsBookingReceived { get; set; }
		ZInt HVH_ItemCount { get; }
		ZGuid HVH_OA_BillToParty { get; set; }
		ZGuid HVH_OA_DispatchAddress { get; set; }
		ZGuid HVH_OA_OriginDepot { get; set; }
		ZGuid HVH_OC_BillToPartyContact { get; set; }
		ZGuid HVH_OC_BookedBy { get; set; }
		ZGuid HVH_OH_FreightAgent { get; set; }
		ZAddress HVH_OA_BillToParty_ZAddress { get; }
		ZAddress HVH_OA_DispatchAddress_ZAddress { get; }
		ZAddress HVH_OA_OriginDepot_ZAddress { get; }
		ZString HVH_RS_NKBookingServiceLevel { get; set; }
		ZDateTime HVH_SystemCreateTimeUtc { get; set; }
		ZString HVH_SystemCreateUser { get; set; }
		ZDateTime HVH_SystemLastEditTimeUtc { get; set; }
		ZString HVH_SystemLastEditUser { get; set; }
		ZBool HVH_UseShipperDeliveryAccount { get; set; }

		ZString BookingStatus { get; set; }
		ZString HumanReadableNameWithoutId { get; }
		ZString WorkflowType { get; }

		Logs Logs { get; }
		Notes Notes { get; }

		IOrgAddress BillToParty { get; }
		IOrgContact BillToPartyContact { get; }
		IOrgContact BookedBy { get; }
		IRefServiceLevel BookingServiceLevel { get; }
		IOrgAddress DispatchAddress { get; }
		IOrgHeader FreightAgent { get; }
		IOrgAddress OriginDepot { get; }
		IHVLVConsignmentCollection Consignments { get; }
		IProcessTaskCollection WorkflowItems { get; }
	}
}
