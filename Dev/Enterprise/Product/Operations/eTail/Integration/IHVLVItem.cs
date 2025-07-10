
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVItem : IBusiness, IHVLVFWBDGCodeProvider
	{
		ZGuid PK { get; }
		ZString HVI_ItemId { get; set; }
		ZString HVI_F3_NKPackType { get; set; }
		ZDecimal HVI_ManifestedWeight { get; set; }
		ZDecimal HVI_ManifestedVolume { get; set; }
		ZGuid HVI_HVL_LoadList { get; set; }
		ZGuid HVI_JS_LoadedOnShipment { get; set; }
		ZInt HVI_ClusterKey { get; set; }
		ZGuid HVI_HVC_Consignment { get; set; }

		ZDecimal HVI_ActualVolume { get; set; }
		ZDecimal HVI_ActualWeight { get; set; }
		ZString HVI_CarrierBookingStatus { get; set; }
		ZString HVI_ContainerNumber { get; set; }
		ZString HVI_CurrentBarcode { get; set; }
		ZDateTime HVI_DestinationFirstUsageTimeUtc { get; set; }
		ZString HVI_GoodsDescription { get; set; }
		ZDecimal HVI_Height { get; set; }
		ZGuid HVI_HVO_OuterPackage { get; set; }
		ZBool HVI_IsActive { get; set; }
		ZBool HVI_IsDamaged { get; set; }
		ZBool HVI_IsPillaged { get; set; }
		ZBool HVI_IsScannedAtDestination { get; set; }
		ZBool HVI_IsUllaged { get; set; }
		ZBool HVI_IsUnmanifestedAtDestination { get; set; }
		ZBool HVI_IsValidatedForUniqueness { get; set; }
		ZGuid HVI_KM_LastMileTransportBooking { get; set; }
		ZDecimal HVI_Length { get; set; }
		ZDateTime HVI_OriginFirstUsageTimeUtc { get; set; }
		ZString HVI_ReleaseStatus { get; set; }
		ZString HVI_ImportReleaseStatus { get; set; }
		ZString HVI_ExportReleaseStatus { get; set; }
		ZDateTime HVI_SecurityFilingFirstUsageTimeUtc { get; set; }
		ZDateTime HVI_ShipperFirstUsageTimeUtc { get; set; }
		ZString HVI_ShipperReference { get; set; }
		ZString HVI_Status { get; set; }
		ZDateTime HVI_SystemCreateTimeUtc { get; set; }
		ZString HVI_SystemCreateUser { get; set; }
		ZDateTime HVI_SystemLastEditTimeUtc { get; set; }
		ZString HVI_SystemLastEditUser { get; set; }
		ZString HVI_UnitOfDimension { get; set; }
		ZString HVI_UsageType { get; set; }
		ZDecimal HVI_Width { get; set; }
		ZString HVI_LastUsageCode { get; set; }
	}
}
