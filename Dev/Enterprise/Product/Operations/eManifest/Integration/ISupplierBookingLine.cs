using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eManifest.Integration
{
	public interface ISupplierBookingLine : IEManifestLine, IStmALogParent
	{
		ZString DL_ConsigneeReference { get; set; }
		ZDecimal DL_Cubic { get; set; }
		ZString DL_CubicUQ { get; set; }
		ZGuid DL_DH_BookingHeader { get; set; }
		ZString DL_GoodsDescription { get; set; }
		ZDecimal DL_GoodsValue { get; set; }
		ZDecimal DL_GrossWeight { get; set; }
		ZString DL_GrossWeightUQ { get; set; }
		ZString DL_MarksAndNumbers { get; set; }
		ZString DL_OrderTrackingNumber { get; set; }
		ZInt DL_PiecesManifested { get; set; }
		ZString DL_RX_NKGoodsValueCurrency { get; set; }
		ZGuid DL_JS_ApprovedShipment { get; set; }
		ZGuid DL_OH_LastMileCarrier { get; set; }
		ZString DL_PL_NKCarrierServiceLevel { get; set; }
		ZGuid DL_OA_DestinationDepot { get; set; }
		ZGuid DL_KM_LastMileTransportBooking { get; set; }
		ZInt DL_Index { get; set; }
		ZBool DL_IsDeliveryTransportSelfBooked { get; set; }
		ZString DL_Status { get; set; }
		ZString DL_ConsigneeName { get; set; }
		ZString DL_ConsigneeAddress1 { get; set; }
		ZString DL_ConsigneeAddress2 { get; set; }
		ZString DL_ConsigneeCity { get; set; }
		ZString DL_ConsigneeState { get; set; }
		ZString DL_ConsigneePostCode { get; set; }
		ZString DL_RN_NKConsigneeCountryCode { get; set; }
		ZString DL_ConsigneeContact { get; set; }
		ZString DL_ConsigneeEmail { get; set; }
		ZString DL_ConsigneePhone { get; set; }
		ZString DL_ConsigneeMobile { get; set; }
		ZString DL_ConsigneeFax { get; set; }
		ZString DL_ConsignorName { get; set; }
		ZString DL_ConsignorAddress1 { get; set; }
		ZString DL_ConsignorAddress2 { get; set; }
		ZString DL_ConsignorCity { get; set; }
		ZString DL_ConsignorState { get; set; }
		ZString DL_ConsignorPostCode { get; set; }
		ZString DL_RN_NKConsignorCountryCode { get; set; }
		ZString DL_ConsignorContact { get; set; }
		ZString DL_ConsignorEmail { get; set; }
		ZString DL_ConsignorPhone { get; set; }
		ZString DL_ConsignorMobile { get; set; }
		ZString DL_ConsignorFax { get; set; }
	}
}
