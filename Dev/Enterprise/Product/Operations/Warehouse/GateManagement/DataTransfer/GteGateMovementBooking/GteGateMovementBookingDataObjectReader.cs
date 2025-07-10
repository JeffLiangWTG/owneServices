using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementBookingDataObjectReader : ShipmentDataObjectReader<GteGateMovementBooking>
	{
		public GteGateMovementBookingDataObjectReader(GteBooking booking, UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.booking = Argument.NotNull(booking, nameof(booking));
		}
		readonly GteBooking booking;

		public override DataContextType DataContextType => DataContextType.GateBooking;

		#region Matching

		protected override IMatchingBusinessEntityFinder<GteGateMovementBooking> GetCombinedReferenceMatcher() => null;

		protected override GteGateMovementBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var referenceNumber = dataObject.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference);
			if (!string.IsNullOrEmpty(referenceNumber))
			{
				var query = new ZQuery(GteGateMovementBookingSchema.GBM_SourceReferenceNumber, referenceNumber)
					.AddToFilter(GteGateMovementBookingSchema.GBM_Source, "VBS")
					.AddToFilter(GteGateMovementBookingSchema.GBM_GBK_Booking, booking.PK);

				return factory.LoadTop1<GteGateMovementBooking>(query);
			}
			return null;
		}

		#endregion Matching

		#region Populate

		protected override void PopulateBusinessObject(GteGateMovementBooking gateMovementBooking)
		{
			SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_BookingReferenceNumber, dataObject.BookingConfirmationReference.Value);

			PopulatePackingLineCollection(gateMovementBooking);
			PopulateContainerCollection(gateMovementBooking);
			PopulateSlotDateTime(gateMovementBooking);
			PopulateGBM_IsPickup(gateMovementBooking);
			PopulateSourceReference(gateMovementBooking);
			PopulateTransportReference(gateMovementBooking);
		}

		void PopulateTransportReference(GteGateMovementBooking gateMovementBooking)
		{
			var additionalReferenceCollection = dataObject.AdditionalReferenceCollection;

			if (additionalReferenceCollection != null)
			{
				var transportReference = additionalReferenceCollection.Where(t => t.Type.Code.Value == AdditionalReferenceTypes.Codes.TransportReference);
				if (transportReference.Any())
				{
					if (transportReference.Count() > 1)
					{
						throw new DataObjectReadFailureException($"Failed to read Additional References. There can not be more than one reference with a 'TRF' code. However there is currently {transportReference.Count()}.");
					}

					SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_TransportReference, transportReference.First().ReferenceNumber);
				}
			}
		}

		void PopulatePackingLineCollection(GteGateMovementBooking gateMovementBooking)
		{
			var packingLineDataObject = dataObject.PackingLineCollection?.FirstOrDefault();

			if (packingLineDataObject != null)
			{
				if (packingLineDataObject.PackType?.Code.HasValue ?? false)
				{
					SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_F3_NKPackageType, packingLineDataObject.PackType.Code.Value);
				}

				if (packingLineDataObject.Commodity?.Code.HasValue ?? false)
				{
					SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_RH_NKCargoType, packingLineDataObject.Commodity.Code.Value);
				}

				if (packingLineDataObject.PackQty != null)
				{
					SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_Quantity, packingLineDataObject.PackQty);
				}
			}
		}

		void PopulateContainerCollection(GteGateMovementBooking gateMovementBooking)
		{
			var containerDataObject = dataObject.ContainerCollection?.FirstOrDefault();
			if (containerDataObject != null)
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_UnitNumber, containerDataObject.ContainerNumber);

				var containerCode = containerDataObject.ContainerType?.Code.Value ?? ZString.Empty;
				if (!containerCode.IsEmpty)
				{
					var containerType = gateMovementBooking.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);
					if (containerType != null)
					{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_RC_UnitType, containerType.PK);
			}
		}
			}
		}

		void PopulateSlotDateTime(GteGateMovementBooking gateMovementBooking)
		{
			if (dataObject.DateCollection == null)
			{
				return;
			}

			var earliestStartTime = dataObject.DateCollection.OrderBy(date => date.Value?.ToZDateTime()).FirstOrDefault(date => date.Type == DateType.Start)?.Value;
			if (earliestStartTime != null)
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_SlotStartTime, earliestStartTime.Value.ZDate is ZDateTimeOffset offset ? offset : new ZDateTimeOffset(earliestStartTime.Value.ZDate));
			}

			var latestEndTime = dataObject.DateCollection.OrderByDescending(date => date.Value?.ToZDateTime()).FirstOrDefault(date => date.Type == DateType.End)?.Value;
			if (latestEndTime != null)
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_SlotEndTime, latestEndTime.Value.ZDate is ZDateTimeOffset offset ? offset : new ZDateTimeOffset(latestEndTime.Value.ZDate));
			}
		}

		void PopulateGBM_IsPickup(GteGateMovementBooking gateMovementBooking)
		{
			if (dataObject.TransportBookingDirection?.Code.Equals(TransportBookingDirections.Codes.Pickup) ?? false)
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_IsPickup, true);
			}
			else if (dataObject.TransportBookingDirection?.Code.Equals(TransportBookingDirections.Codes.Delivery) ?? false)
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_IsPickup, false);
			}
			else
			{
				var errorMsg = Res.GetString(
					"5fdc045f-ab4d-58b2-428c-8be42e716d7f",
					"Failed to read 'Transport Booking Direction'. Value must be either '{0}' or '{1}'.",
					TransportBookingDirections.Codes.Delivery,
					TransportBookingDirections.Codes.Pickup);

				throw new DataObjectReadFailureException(errorMsg);
			}
		}

		void PopulateSourceReference(GteGateMovementBooking gateMovementBooking)
		{
			var reference = dataObject.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference);
			if (!string.IsNullOrEmpty(reference))
			{
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_Source, "VBS");
				SetValue(gateMovementBooking, GteGateMovementBookingSchema.GBM_SourceReferenceNumber, reference);
			}
		}

		#endregion Populate
	}
}
