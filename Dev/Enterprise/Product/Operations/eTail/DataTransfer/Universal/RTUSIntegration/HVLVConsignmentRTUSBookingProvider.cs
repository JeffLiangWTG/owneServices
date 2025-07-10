using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using WTG.RTUS.Interface;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVConsignmentRTUSBookingProvider : RTUSBookingProvider<HVLVConsignment>
	{
		public HVLVConsignmentRTUSBookingProvider(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Overrides

		protected override ItemShipment[] CreateUniversalShipments(HVLVConsignment consignment, RequestType requestType)
		{
			var shipments = new List<ItemShipment>();
			var dataWritingManager = new DataWritingManager(new ActionInfo(null, consignment));

			foreach (HVLVItem item in consignment.Items)
			{
				var writer = new RTUSConsignmentDataObjectWriter(
					dataWritingManager,
					item.PK,
					requestType);
				var universalShipment = writer.GetDataObject(consignment);
				shipments.Add(new ItemShipment(item.HVI_ItemId, item, universalShipment));
			}

			return shipments.ToArray();
		}

		protected override ZGuid GetBookingAgentPK(HVLVConsignment consignment)
		{
			return consignment.HVC_OH_LastMileCarrierBookingAgent;
		}

		protected override void OnBooked(RTUSServiceResponse<ISingleBookingRTUSResponse> response)
		{
			response.Item.HVI_CurrentBarcode = response.Response.TrackingNumber;
			Factory.Save();
		}

		#endregion

		#region FormattedLogMessage

		protected override string GetBookingConfirmedReferenceText<TResponse>(HVLVConsignment bizo, RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier|REF={serviceResponse.Reference}"); // For logging purpose only
		}

		protected override string GetBookingRejectedReferenceText<TResponse>(HVLVConsignment bizo, RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier|REF={serviceResponse.Reference}"); // For logging purpose only
		}

		protected override string GetBookingCancelledReferenceText<TResponse>(HVLVConsignment bizo, RTUSServiceResponse<TResponse> serviceResponse)
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier Booking|REF={serviceResponse.Reference}"); // For logging purpose only
		}

		#endregion
	}
}
