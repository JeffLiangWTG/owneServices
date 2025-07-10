using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignment : IEManifestLine, IBusiness
	{
		ZString HVC_ConsignmentId { get; set; }
		ZString HVC_WaybillNumber { get; set; }
		ZString HVC_Status { get; set; }
		ZString HVC_ShipperReference { get; set; }
		ZGuid HVC_HVH_BookingHeader { get; set; }
		ZGuid HVC_JE_ImportDeclaration { get; set; }
		ZGuid HVC_JE_ExportDeclaration { get; set; }
		ZGuid HVC_JS_ManifestedOnShipment { get; set; }

		ZString HVC_ACASInterchangeStatus { get; set; }
		ZString HVC_ACASMessageStatus { get; set; }
		ZString HVC_ACASStatus { get; set; }
		ZDecimal HVC_ActualVolume { get; set; }
		ZDecimal HVC_ActualWeight { get; set; }
		ZBool HVC_AuthorityToLeave { get; set; }
		ZString HVC_CarrierAccountNumber { get; set; }
		ZInt HVC_ClusterKey { get; set; }
		ZString HVC_ConsigneeAddress1 { get; set; }
		ZString HVC_ConsigneeAddress2 { get; set; }
		ZString HVC_ConsigneeAddressValidationStatus { get; set; }
		ZString HVC_ConsigneeCity { get; set; }
		ZString HVC_ConsigneeContact { get; set; }
		ZString HVC_ConsigneeEmail { get; set; }
		ZString HVC_ConsigneeFax { get; set; }
		ZString HVC_ConsigneeInstructions { get; set; }
		ZString HVC_ConsigneeMobile { get; set; }
		ZString HVC_ConsigneeName { get; set; }
		ZString HVC_ConsigneePhone { get; set; }
		ZString HVC_ConsigneePostcode { get; set; }
		ZString HVC_ConsigneeState { get; set; }
		ZString HVC_ExportCustomsClearanceStatus { get; set; }
		ZString HVC_GoodsDescription { get; set; }
		ZDecimal HVC_GoodsValue { get; set; }
		ZString HVC_ImportCustomsClearanceStatus { get; set; }
		ZString HVC_INCO { get; set; }
		ZBool HVC_IsActive { get; set; }
		ZBool HVC_IsHazardous { get; set; }
		ZBool HVC_IsPerishable { get; set; }
		ZBool HVC_IsPersonalEffects { get; set; }
		ZBool HVC_IsSelfBooked { get; set; }
		ZBool HVC_IsSignatureRequired { get; set; }
		ZBool HVC_IsTaxPrePaid { get; set; }
		ZBool HVC_IsTimber { get; set; }
		ZBool HVC_IsValidatedForUniqueness { get; set; }
		ZShort HVC_ItemCount { get; set; }
		ZDecimal HVC_ManifestedVolume { get; set; }
		ZDecimal HVC_ManifestedWeight { get; set; }
		ZGuid HVC_OA_ConsigneeAddress { get; set; }
		ZGuid HVC_OA_DestinationDepot { get; set; }
		ZGuid HVC_OA_ShipperAddress { get; set; }
		ZGuid HVC_OH_LastMileCarrier { get; set; }
		ZGuid HVC_OH_LastMileCarrierBookingAgent { get; set; }
		ZString HVC_PL_NKLastMileCarrierServiceLevel { get; set; }
		ZString HVC_PreScreeningStatus { get; set; }
		ZString HVC_ReleaseStatus { get; set; }
		ZString HVC_ImportReleaseStatus { get; set; }
		ZString HVC_ExportReleaseStatus { get; set; }
		ZBool HVC_RequiresFumigation { get; set; }
		ZString HVC_RN_NKConsigneeCountryCode { get; set; }
		ZString HVC_RN_NKShipperCountryCode { get; set; }
		ZString HVC_RX_NKGoodsValueCurrency { get; set; }
		ZString HVC_ShipperAddress1 { get; set; }
		ZString HVC_ShipperAddress2 { get; set; }
		ZString HVC_ShipperCity { get; set; }
		ZString HVC_ShipperContact { get; set; }
		ZString HVC_ShipperEmail { get; set; }
		ZString HVC_ShipperFax { get; set; }
		ZString HVC_ShipperMobile { get; set; }
		ZString HVC_ShipperName { get; set; }
		ZString HVC_ShipperPhone { get; set; }
		ZString HVC_ShipperPostcode { get; set; }
		ZString HVC_ShipperState { get; set; }
		ZDateTime HVC_SystemCreateTimeUtc { get; set; }
		ZString HVC_SystemCreateUser { get; set; }
		ZDateTime HVC_SystemLastEditTimeUtc { get; set; }
		ZString HVC_SystemLastEditUser { get; set; }
		ZString HVC_UndgClass { get; set; }
		ZString HVC_VendorIdentifier { get; set; }
		ZString HVC_VolumeUQ { get; set; }
		ZString HVC_WeightUQ { get; set; }

		IHVLVItemCollection Items { get; }
	}
}
