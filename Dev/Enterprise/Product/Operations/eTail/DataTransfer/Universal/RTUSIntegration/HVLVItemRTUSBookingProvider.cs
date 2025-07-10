using System;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using WTG.RTUS.Interface;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVItemRTUSBookingProvider : RTUSBookingProvider<HVLVItem>
	{
		public HVLVItemRTUSBookingProvider(BusinessObjectFactory factory) : base(factory) { }

		#region Overrides

		protected override ItemShipment[] CreateUniversalShipments(HVLVItem item, RequestType requestType)
		{
			var consignment = item.Consignment;
			var dataWritingManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new RTUSConsignmentDataObjectWriter(
				dataWritingManager,
				item.PK,
				requestType);
			var universalShipment = writer.GetDataObject(consignment);

			return new[] { new ItemShipment(item.HVI_ItemId, item, universalShipment) };
		}

		protected override ZGuid GetBookingAgentPK(HVLVItem item)
		{
			return item.Consignment.HVC_OH_LastMileCarrierBookingAgent;
		}

		protected override void OnBooked(RTUSServiceResponse<ISingleBookingRTUSResponse> response)
		{
			var item = response.Item;
			item.HVI_CurrentBarcode = response.Response.TrackingNumber;
			item.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingConfirmed;
			Factory.Save();
		}

		protected override void OnRejected(RTUSServiceResponse<ISingleBookingRTUSResponse> response)
		{
			response.Item.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingRejected;
			Factory.Save();
		}

		protected override void OnCanceled(HVLVItem item)
		{
			item.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingCancelled;
			Factory.Save();
		}

		#endregion

		#region FormattedLogMessage

		protected override string GetBookingConfirmedReferenceText<TResponse>(HVLVItem bizo,
			RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier|OLD={bizo.HVI_CurrentBarcode}|NEW={((ISingleBookingRTUSResponse)serviceResponse.Response).TrackingNumber}"); // For logging purpose only
		}

		protected override string GetBookingCancelledReferenceText<TResponse>(HVLVItem bizo, RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier Booking|REF={bizo.HVI_CurrentBarcode}"); // For logging purpose only
		}

		protected override string GetBookingRejectedReferenceText<TResponse>(HVLVItem bizo, RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier|REF={serviceResponse.Reference}"); // For logging purpose only
		}

		#endregion
	}
}
