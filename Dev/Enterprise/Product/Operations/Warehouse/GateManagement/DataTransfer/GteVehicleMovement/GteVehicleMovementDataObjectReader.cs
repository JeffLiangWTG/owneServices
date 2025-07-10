using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using GateManagementConstants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleMovementDataObjectReader : ShipmentDataObjectReader<GteVehicleMovement>
	{
		public GteVehicleMovementDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.GateVehicleMovement;

		protected override IMatchingBusinessEntityFinder<GteVehicleMovement> GetCombinedReferenceMatcher() => null;

		protected override GteVehicleMovement GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();
			if (dataTarget != null && dataTarget.Key.HasValue)
			{
				var entry = factory.LoadTop1<GteVehicleEntry>(new ZQuery(GteVehicleEntrySchema.GVE_GateActionNumber, dataTarget.Key));
				if (entry != null)
				{
					return entry.VehicleMovement;
				}
			}

			var movementBookings = GetMatchingGateMovementBookingsByMovementBookingNumber();
			var gateMovements = movementBookings.SelectMany(booking => booking.GateMovements);
			var vehicleMovement = gateMovements.FirstOrDefault(gateMovement => !gateMovement.VehicleMovement.IsNull)?.VehicleMovement;
			return vehicleMovement;
		}

		protected override void PopulateBusinessObject(GteVehicleMovement vehicleMovement)
		{
			if (dataObject.SubShipmentCollection == null || dataObject.SubShipmentCollection.Count == 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("2d5d76e7-7491-4227-baf0-4d98a82f3658", "Vehicle Movement must contain at least one Gate Movement"));
			}

			PopulateVehicleData(vehicleMovement);
			PopulateLocation(vehicleMovement);
			PopulateVehicleEntries(vehicleMovement);
			PopulateGateMovements(vehicleMovement);

			if (!vehicleMovement.GVM_GBK_MainBooking.IsValid)
			{
				SetValue(vehicleMovement, GteVehicleMovementSchema.GVM_GBK_MainBooking, GetOrCreateParentBooking(vehicleMovement).PK);
			}
		}

		void PopulateVehicleData(GteVehicleMovement vehicleMovement)
		{
			var vehicleData = dataObject.VehicleRun?.Vehicle;
			var vehicleRegistration = vehicleData?.Registration?.Number;
			var vehicleTypeCode = vehicleData?.VehicleType?.Code;
			var vehicleType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, vehicleTypeCode ?? "");

			if (string.IsNullOrEmpty(vehicleRegistration) || vehicleType == null)
			{
				var associatedBooking = GetParentBookingIfExists();
				var vehicleBooking = associatedBooking?.VehicleMovementBookings.FirstOrDefault() as GteVehicleMovementBooking;
				if (vehicleBooking != null)
				{
					if (string.IsNullOrEmpty(vehicleRegistration) && string.IsNullOrEmpty(vehicleMovement.GVM_VehicleRegistration))
					{
						vehicleRegistration = vehicleBooking.GBV_VehicleRegistration;
					}
					if (vehicleType == null && vehicleMovement.VehicleType == null)
					{
						vehicleType = vehicleBooking.VehicleType;
					}
				}
			}

			if (!string.IsNullOrEmpty(vehicleRegistration))
			{
				SetValue(vehicleMovement, GteVehicleMovementSchema.GVM_VehicleRegistration, vehicleRegistration);
			}

			if (vehicleType != null)
			{
				SetValue(vehicleMovement, GteVehicleMovementSchema.GVM_RC_VehicleType, vehicleType?.PK ?? ZGuid.Empty);
			}

			if (string.IsNullOrEmpty(vehicleMovement.GVM_VehicleRegistration))
			{
				throw new DataObjectReadFailureException(Res.GetString("fb309181-8625-4f32-87a5-fe2f7b224de2", "Cannot save a Vehicle Movement without a Registration"));
			}
		}

		void PopulateLocation(GteVehicleMovement vehicleMovement)
		{
			var locationString = dataObject.WarehouseLocation.ToString();
			try
			{
				var facility = FacilityMatchingHelper.GetFacility(dataObject, factory, logger);
				var warehouse = factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, facility.GetValue(WhsWarehouseSchema.PK)));
				if (warehouse != null)
				{
					var location = warehouse.FindLocation(locationString);
					if (location != null)
					{
						SetValue(vehicleMovement, GteVehicleMovementSchema.GVM_WL_Location, location.PK);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		void PopulateVehicleEntries(GteVehicleMovement vehicleMovement)
		{
			var vehicleEntryShipments = dataObject.RelatedShipmentCollection;
			var vehicleEntryDates = dataObject.DateCollection;
			if (!vehicleEntryShipments.IsNullOrEmpty())
			{
				var booking = GetOrCreateParentBooking(vehicleMovement);
				var collectionReader = new GteVehicleEntryCollectionDataObjectReader(vehicleMovement, booking, logger, factory, vehicleEntryShipments.ToArray());
				collectionReader.ReadIntoCollectionRetainingUnmatchedElements();

				var gateInTime = vehicleEntryDates?.FirstOrDefault(date => date.Type == DateType.Start)?.Value;
				if (gateInTime != null && vehicleMovement.GateInVehicleEntry != null)
				{
					vehicleMovement.GateInVehicleEntry.GVE_EntryTime = gateInTime.Value.ZDate is ZDateTimeOffset offset ? offset : new ZDateTimeOffset(gateInTime.Value.ZDate);
				}

				var gateOutTime = vehicleEntryDates?.FirstOrDefault(date => date.Type == DateType.End)?.Value;
				if (gateOutTime != null && vehicleMovement.GateOutVehicleEntry != null)
				{
					vehicleMovement.GateOutVehicleEntry.GVE_EntryTime = gateOutTime.Value.ZDate is ZDateTimeOffset offset ? offset : new ZDateTimeOffset(gateOutTime.Value.ZDate);
				}
			}
		}

		void PopulateGateMovements(GteVehicleMovement vehicleMovement)
		{
			var gateMovementList = dataObject.SubShipmentCollection;
			if (gateMovementList == null || gateMovementList.Count == 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("104bafee-c0d2-4bac-b0d4-a68baf08ceaa", "Vehicle Movement must contain at least one Gate Movement"));
			}

			var booking = GetOrCreateParentBooking(vehicleMovement);
			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, booking, logger, factory, gateMovementList.ToArray());
			collectionReader.ReadIntoCollectionRetainingUnmatchedElements();
		}

		GteBooking GetOrCreateParentBooking(GteVehicleMovement vehicleMovement)
		{
			if (GetParentBookingIfExists() == null)
			{
				parentBooking = factory.New<GteBooking>();

				var transportCompanyPK = dataObject.GetOrganizationAddressForShipment(logger, factory, DocAddressType.TransportCompanyDocumentaryAddress).OA_OH;
				var facility = FacilityMatchingHelper.GetFacility(dataObject, factory, logger);

				SetValue(parentBooking, GteBookingSchema.GBK_OH_TransportCompany, transportCompanyPK);
				SetValue(parentBooking, GteBookingSchema.GBK_WW_Facility, facility.GetValue(WhsWarehouseSchema.PK));
				SetValue(parentBooking, GteBookingSchema.GBK_BookingType, GateManagementConstants.BookingTypes.AdHoc);
			}
			return parentBooking;
		}

		GteBooking GetParentBookingIfExists()
		{
			if (parentBooking == null)
			{
				var movementBookings = GetMatchingGateMovementBookingsByMovementBookingNumber();
				var movementBookingsOrdered = movementBookings
					.OrderBy(gbm => gbm.GateMovements.Any(ggm => string.IsNullOrEmpty(ggm.GGM_CancelledReason)) ? 0 : 1)
					.ThenBy(gbm => gbm.Booking.GBK_BookingType.Equals(GateManagementConstants.BookingTypes.AdHoc) ? 1 : 0)
					.ThenBy(gbm => gbm.GBM_SlotStartTime.IsValid ? 0 : 1)
					.ThenBy(gbm => gbm.GBM_SlotStartTime);
				var booking = movementBookings.FirstOrDefault()?.Booking;
				if (booking != null)
				{
					parentBooking = booking;
				}
			}
			return parentBooking;
		}

		GteBooking parentBooking;

		GteGateMovementBooking[] GetMatchingGateMovementBookingsByMovementBookingNumber()
		{
			if (matchingGateMovementBookings == null)
			{
				var movementBookingNumbers = dataObject.SubShipmentCollection?.Select(shipment => shipment.GetAdditionalReferenceOrDefault(GateManagementConstants.ReferenceTypes.Codes.MovementBookingNumber));
				movementBookingNumbers = movementBookingNumbers.Where(reference => !string.IsNullOrEmpty(reference));
				if (movementBookingNumbers.Any())
				{
					var query = new ZQuery().AddToFilter(JoinCondition.Or, GteGateMovementBookingSchema.GBM_MovementBookingNumber, movementBookingNumbers);
					matchingGateMovementBookings = factory.Load<GteGateMovementBooking>(query);
				}
				else
				{
					matchingGateMovementBookings = Array.Empty<GteGateMovementBooking>();
				}
			}

			return matchingGateMovementBookings;
		}
		GteGateMovementBooking[] matchingGateMovementBookings;
	}
}
