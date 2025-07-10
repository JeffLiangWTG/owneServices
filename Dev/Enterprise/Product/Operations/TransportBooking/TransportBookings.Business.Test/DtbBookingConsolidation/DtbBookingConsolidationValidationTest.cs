using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Test
{
	class DtbBookingConsolidationValidationTest : DtbTransportConsolidationValidationTest
	{
		public void TestValidateBookingTransportConsolidation()
		{
			var transportCo = Helper.CreateOrganisation("QQW");
			transportCo.MiscServ.OM_TBAllowMixedAccountNumbersOnManifest = true;

			var booking1 = Helper.CreateBooking(transportCo);
			booking1.KM_OAN_CarrierAccount = ZGuid.NewZGuid();

			var booking2 = Helper.CreateBooking(transportCo);
			booking2.KM_OAN_CarrierAccount = ZGuid.NewZGuid();

			var consol = Helper.CreateConsolidation();
			consol.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			consol.Address.E2_OA_Address = transportCo.MainAddress.PK;
			consol.Bookings.Add(booking1);
			consol.Bookings.Add(booking2);

			consol.Validation.ValidateAll();
			AssertNoRowError(consol, "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier.");

			transportCo.MiscServ.OM_TBAllowMixedAccountNumbersOnManifest = false;
			consol.Validation.ValidateAll();
			AssertHasRowError(consol, "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier.");

			consol.Bookings.RemoveFromRelationship(booking1);
			consol.Validation.ValidateAll();
			AssertNoRowError(consol, "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier.");

			transportCo.MiscServ.OM_TBAllowMixedAccountNumbersOnManifest = true;
			consol.Validation.ValidateAll();
			AssertNoRowError(consol, "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier.");
		}

		public void TestValidateNonBookingTransportConsolidation()
		{
			var transportCo = Helper.CreateOrganisation("QQW");

			var booking1 = Helper.CreateBooking(transportCo);
			booking1.KM_OAN_CarrierAccount = ZGuid.NewZGuid();

			var booking2 = Helper.CreateBooking(transportCo);
			booking2.KM_OAN_CarrierAccount = ZGuid.NewZGuid();

			var consol = Helper.CreateConsolidation();
			consol.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			consol.Bookings.Add(booking1);
			consol.Bookings.Add(booking2);

			consol.Validation.ValidateAll();
			AssertNoRowError(consol, "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier.");
		}

		public void TestValidateMasterAttachment_KB_JobDirection()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var matchingConsolidation = Helper.CreateConsolidation();
			var nonMatchingConsolidation = Helper.CreateConsolidation();

			matchingConsolidation.KB_JobDirection = masterConsolidation.KB_JobDirection = "PIC";
			nonMatchingConsolidation.KB_JobDirection = "DLV";

			Factory.Save();

			var subConsolidations = new DtbBookingConsolidation[] { matchingConsolidation, nonMatchingConsolidation };
			subConsolidations.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors(matchingConsolidation);
			AssertNoErrors(nonMatchingConsolidation);

			matchingConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			matchingConsolidation.KB_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			nonMatchingConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			nonMatchingConsolidation.KB_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			subConsolidations.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should accept the sub with matching KB_JobDirection", matchingConsolidation);
			AssertHasError("Should say the sub consolidation is invalid because KB_JobDirection does not match the master", nonMatchingConsolidation.KB_JobDirectionInfo, "This Consolidation has a different job direction from the Master Consolidation.");

			nonMatchingConsolidation.KB_KB_MasterBookingConsolidation = ZGuid.Empty;
			nonMatchingConsolidation.KB_MasterBookingVersion = 0;
			Factory.Save();

			subConsolidations.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should accept the sub with matching KB_JobDirection", matchingConsolidation);

			masterConsolidation.KB_JobDirection = "ORG";
			subConsolidations.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when master changes", matchingConsolidation);
		}

		public void TestValidateTBHasAccessToJobWhenParentJobIsForwardingConsolidation()
		{
			var shipment = Helper.CreateForwardingShipment();
			var forwardingConsol = Helper.CreateForwardingConsol(shipment, "", "", "ccn88884444");
			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsol);
			var booking = Helper.CreateBooking(tbConsol);
			var consolShipmentProvider = new ConsolShipmentProvider(booking.ConsolidationSingleJob.ParentBO);

			Factory.Save();

			AssertEquals("Precondition: Booking Parent Job Type must be a Forwarding Consolidation", booking.ConsolidationSingleJob.Parent.JobType, "CON");
			AssertEquals("Precondition: Forwarding Consolidation has Shipment", consolShipmentProvider.Shipments.Count, 1);

			tbConsol.Validation.ValidateAll();
			AssertNoRowWarnings(booking);

			forwardingConsol = Helper.CreateForwardingConsol(null, "", "", "ccn88885555");
			tbConsol = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsol);
			booking = Helper.CreateBooking(tbConsol);

			Factory.Save();

			consolShipmentProvider = new ConsolShipmentProvider(booking.ConsolidationSingleJob.ParentBO);
			AssertEquals("Precondition: Booking Parent Job Type must be a Forwarding Consolidation", booking.ConsolidationSingleJob.Parent.JobType, "CON");
			AssertEquals("Precondition: Forwarding Consolidation has no Shipment", consolShipmentProvider.Shipments.Count, 0);

			tbConsol.Validation.ValidateAll();
			AssertHasRowWarning(booking, $"Does not have a valid Shipment. Costs cannot be apportioned to this record.");

			tbConsol = Helper.CreateConsolidation();
			booking = Helper.CreateBooking(tbConsol);
			tbConsol.Bookings.Add(booking);

			Factory.Save();

			AssertNotEquals("Precondition: Booking Parent Job type not CON", booking.ConsolidationSingleJob?.Parent?.JobType, "CON");

			tbConsol.Validation.ValidateAll();
			AssertNoRowWarnings(booking);
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
