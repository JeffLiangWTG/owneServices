using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteBookingDataObjectReader : ShipmentDataObjectReader<GteBooking>
	{
		public GteBookingDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.GateBooking;

		#region Matching

		protected override IMatchingBusinessEntityFinder<GteBooking> GetCombinedReferenceMatcher() => null;

		protected override GteBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();

			if (dataTarget != null && dataTarget.Key.HasValue)
			{
				var booking = factory.LoadTop1<GteBooking>(new ZQuery(GteBookingSchema.GBK_ReferenceNumber, dataTarget.Key));
				if (booking != null)
				{
					return booking;
				}
			}

			var subShipment = dataObject.SubShipmentCollection?.FirstOrDefault();
			var referenceNumber = subShipment?.GetAdditionalReferenceOrDefault(AdditionalReferenceTypes.Codes.BookingPartyReference);
			if (!string.IsNullOrEmpty(referenceNumber))
			{
				var query = new ZQuery(GteGateMovementBookingSchema.GBM_SourceReferenceNumber, referenceNumber).AddToFilter(GteGateMovementBookingSchema.GBM_Source, "VBS");
				var movementBooking = factory.LoadTop1<GteGateMovementBooking>(query);
				return movementBooking?.Booking;
			}

			return null;
		}

		#endregion Matching

		#region Populate

		protected override void PopulateBusinessObject(GteBooking booking)
		{
			var vehicleEntriesForBooking = booking.GateMovementBookings
				.SelectMany(movementBooking => movementBooking.GateMovements)
				.Select(gateMovement => gateMovement.VehicleMovement)
				.SelectMany(vehicleMovement => vehicleMovement.VehicleEntries);

			if (vehicleEntriesForBooking.Any(vehicleEntry => vehicleEntry.GVE_CancelledReason == ""))
			{
				var message = Res.GetString("0cba2e2b-6726-4dcf-8e90-b4c8c420a3c7", "Cannot update Booking details after it has arrived in a facility.");
				throw new DataObjectReadFailureException(message);
			}

			SetValue(booking, GteBookingSchema.GBK_BookingType, BookingTypes.Regular);

			PopulateOrganizationAddressCollection(booking);
			PopulateGateMovementBookings(booking);
			PopulateVehicleMovementBookings(booking);
			PopulateVehicleDriverBookings(booking);
			PopulateFacility(booking);

			CreateFacilityJobs(booking);
		}

		void CreateFacilityJobs(GteBooking booking)
		{
			var facilityFactory = new GteFacilityJobIntegrationHelper();
			facilityFactory.CreateAndLinkFacilityJobs(dataObject, logger, factory, booking);
		}

		void PopulateOrganizationAddressCollection(GteBooking booking)
		{
			var transportCompanyAddress = dataObject.GetOrganizationAddressForShipment(logger, factory, DocAddressType.TransportCompanyDocumentaryAddress);
			SetValue(booking, GteBookingSchema.GBK_OH_TransportCompany, transportCompanyAddress.OA_OH);

			var bookingPartyOrganizationAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType?.ToString() == nameof(DocAddressType.BookingPartyDocumentaryAddress));
			if (bookingPartyOrganizationAddress != null)
			{
				var gateManagementOrganisationDataObjectReader = new GateManagementOrganisationDataObjectReader(bookingPartyOrganizationAddress, logger, factory);
				var matchedBookingPartyOrgAddress = gateManagementOrganisationDataObjectReader.GetMatched();
				if (matchedBookingPartyOrgAddress != null)
				{
					var bookingPartyJobDocAddress = JobDocAddress.New(booking, DocAddressType.BookingPartyDocumentaryAddress);
					gateManagementOrganisationDataObjectReader.PopulateJobDocAddress(matchedBookingPartyOrgAddress, bookingPartyJobDocAddress);
					booking.DocAddresses.Add(bookingPartyJobDocAddress);
				}
				else
				{
					var errorMessage = Res.GetString("ec1a52cd-3577-4125-8176-cde68874d2a6", "No matching organization address found for the booking party.");
					throw new DataObjectReadFailureException(errorMessage);
				}
			}
		}

		void PopulateGateMovementBookings(GteBooking booking)
		{
			var subShipmentList = dataObject.SubShipmentCollection;
			if (subShipmentList == null || subShipmentList.Count < 1)
			{
				var errorMessage = Res.GetString("42afd85a-0c35-27b5-4665-5e3f46183275", "Booking must contain at least one Gate Movement Booking.");
				logger.Log(LogType.Error, errorMessage);
				throw new DataObjectReadFailureException(errorMessage);
			}

			var collectionReader = new GteGateMovementBookingCollectionDataObjectReader(booking, logger, factory, subShipmentList.ToArray());
			collectionReader.ReadIntoCollection();
		}

		void PopulateVehicleMovementBookings(GteBooking booking)
		{
			var vehicleMovementBookingList = dataObject.PreCarriageShipmentCollection;
			if (vehicleMovementBookingList == null || vehicleMovementBookingList.Count < 1)
			{
				return;
			}

			var collectionReader = new GteVehicleMovementBookingCollectionDataObjectReader(booking, logger, factory, vehicleMovementBookingList.ToArray());
			collectionReader.ReadIntoCollection();
		}

		void PopulateVehicleDriverBookings(GteBooking booking)
		{
			booking.VehicleDriverBookings.RemoveAndDeleteAll();

			var vehicleDriverBookingList = dataObject.VehicleRun?.CrewCollection;
			if (vehicleDriverBookingList == null || vehicleDriverBookingList.Count < 1)
			{
				return;
			}

			var collectionReader = new GteVehicleDriverBookingCollectionDataObjectReader(booking, dataObject, logger, factory, vehicleDriverBookingList.ToArray());
			collectionReader.ReadIntoCollection();
		}

		void PopulateFacility(GteBooking booking)
		{
			var facility = FacilityMatchingHelper.GetFacility(dataObject, factory, logger);
			SetValue(booking, GteBookingSchema.GBK_WW_Facility, facility.GetValue(WhsWarehouseSchema.PK));
		}

		#endregion Populate
	}
}
