using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	public static class BookingToTransportJobCreatorTestHelper
	{
		public static DtbBooking CreateNewBookingValidForManuallyCreatingATransportJobFrom(TransportBookingTestHelper helper, IDtbBookingParent parent = null, string consolidationJobDirection = "PIC")
		{
			var booking = CreateNewBooking(helper, GlbCompany.CurrentCompany.OrgProxy, parent, consolidationJobDirection, bookingIsActive: true, transportCompanyIsRoadTransport: true, transportCompanyIsCarrier: true);

			CreateNewInstruction(helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			CreateNewInstruction(helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			return booking;
		}

		public static DtbBooking CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(TransportBookingTestHelper helper, DtbBookingConsolidation consolidation)
		{
			var booking = CreateNewBookingValidForManuallyCreatingATransportJobFrom(helper);
			consolidation.Bookings.Add(booking);

			return booking;
		}

		public static DtbBooking MakeBookingValidForManuallyCreatingATransportJobFrom(TransportBookingTestHelper helper, DtbBooking booking)
		{
			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			booking.KM_IsActive = true;
			booking.Address.Organisation.OH_IsShippingProvider = true;
			booking.Address.Organisation.OH_IsLocalTransport = true;

			booking.Instructions.DeleteAll();

			CreateNewInstruction(helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			CreateNewInstruction(helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			return booking;
		}

		public static void SetDatesRelatedToAutomaticTransportJobCreationForBooking(BusinessObjectFactory factory, DtbBooking booking, ZDateTime lastEditedTimeForBooking, ZDateTime requiredFromDateForPickupConfirmation)
		{
			factory.Saved += (f, wasSuccessful) =>
			{
				//setting utc time in DB
				((IDbConnected)factory).Connection.ExecuteNonQuery(string.Format("UPDATE dbo.DtbBooking SET KM_SystemLastEditTimeUtc = '{0}' WHERE KM_PK = '{1}'",
				lastEditedTimeForBooking.ToISO8601String(), booking.PK.ToString()));
			};

			var pickupConfirmationOnBooking = booking.PickupConfirmations.First();

			if (pickupConfirmationOnBooking != null)
			{
				pickupConfirmationOnBooking.KK_RequiredFrom = requiredFromDateForPickupConfirmation;
			}
		}

		public static DtbBooking CreateNewBooking(TransportBookingTestHelper helper, OrgHeader transportCompany, IDtbBookingParent parent, string consolidationJobDirection = "PIC", bool bookingIsActive = true, bool transportCompanyIsRoadTransport = true, bool transportCompanyIsCarrier = true)
		{
			var consolidation = parent == null ? helper.CreateConsolidation() : helper.CreateConsolidation(parent);

			consolidation.KB_JobDirection = consolidationJobDirection;

			return CreateNewBookingOnConsolidation(transportCompany, consolidation, bookingIsActive, transportCompanyIsRoadTransport, transportCompanyIsCarrier);
		}

		public static DtbBooking CreateNewBookingOnConsolidation(OrgHeader transportCompany, DtbBookingConsolidation consolidation, bool bookingIsActive = true, bool transportCompanyIsRoadTransport = true, bool transportCompanyIsCarrier = true)
		{
			var booking = consolidation.Bookings.AddNew();

			booking.Address.OrganisationPK = transportCompany.PK;
			booking.KM_IsActive = bookingIsActive;
			booking.Address.Organisation.OH_IsShippingProvider = transportCompanyIsCarrier;
			booking.Address.Organisation.OH_IsLocalTransport = transportCompanyIsRoadTransport;

			TestCase.AssertEquals("Precondition: Booking Address should be a Transport Company Documentary Address.", DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, booking.Address.E2_AddressType);

			return booking;
		}

		public static DtbBookingInstruction CreateNewInstruction(TransportBookingTestHelper helper, DtbBooking booking, ZString instructionType, ZString orgType, OrgAddress address, bool shouldHaveAtLeastOneConfirmation = true, bool attachExistingPackageOrCreateAndAttachPackage = true)
		{
			var instruction = helper.CreateInstruction(booking, instructionType, orgType, address);
			
			if (address == null)
			{
				instruction.Address.E2_OA_Address = ZGuid.Empty;
			}

			if (attachExistingPackageOrCreateAndAttachPackage)
			{
				PkgPackage packageToAttach = null;

				if (booking.PackageJob.Packages.Any())
				{
					packageToAttach = booking.PackageJob.Packages[0];
				}
				else
				{
					packageToAttach = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew();
				}

				helper.CreatePackageDivot(instruction, packageToAttach);
			}

			if (shouldHaveAtLeastOneConfirmation)
			{
				if (!instruction.Confirmations.Any())
				{
					helper.CreateConfirmation(instruction, instructionType);
				}
			}
			else
			{
				instruction.Confirmations.DeleteAll();
			}

			return instruction;
		}

		public static void ForceBookingConsolidationToConsignmentConsolidationInDB(DbConnection connection, ZGuid consolPK)
		{
			var sqlToSetConsolJobTypeToCSN = $@"
				IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_PreventWrongJobTypeInBookingConsolidation')
					ALTER TABLE dbo.DtbBookingConsolidation DISABLE TRIGGER TG_PreventWrongJobTypeInBookingConsolidation
				IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_PreventWrongJobTypeInBooking')
					ALTER TABLE dbo.DtbBooking DISABLE TRIGGER TG_PreventWrongJobTypeInBooking

				UPDATE dbo.DtbBookingConsolidation SET KB_JobType = 'CSN'
					WHERE KB_PK = '{consolPK}'

				UPDATE dbo.DtbBooking SET KM_JobType = 'CSN'
					WHERE KM_KB_Booking = '{consolPK}'

				IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_PreventWrongJobTypeInBookingConsolidation')
					ALTER TABLE dbo.DtbBookingConsolidation ENABLE TRIGGER TG_PreventWrongJobTypeInBookingConsolidation
				IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_PreventWrongJobTypeInBooking')
					ALTER TABLE dbo.DtbBooking ENABLE TRIGGER TG_PreventWrongJobTypeInBooking
				";

			using (var command = connection.Command(sqlToSetConsolJobTypeToCSN))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
