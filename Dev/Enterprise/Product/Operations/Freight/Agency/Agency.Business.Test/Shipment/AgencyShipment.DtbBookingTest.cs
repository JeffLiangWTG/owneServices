using System;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class AgencyShipmentTest
	{
		public void TestDtbBookingParentControllerID()
		{
			var shipment = Factory.New<AgencyShipment>();
			IDtbBookingParent parent = shipment;
			ControllerID x;
			AssertExceptionThrown(typeof(NotImplementedException), () => x = parent.ControllerID);
		}

		public void TestDTB_JobDetails()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "V00000100";
			shipment.JS_GoodsDescription = "Shipment Description";
			shipment.JS_HouseBill = "BILL1";
			shipment.JS_ShipmentStatus = "TST";

			Factory.Save();

			IDtbBookingParent parent = shipment;

			CombineAssertions(delegate
			{
				AssertEquals("JobNumber", "V00000100", parent.JobNumber);
				AssertEquals("Job description", "Shipping Shipment V00000100", parent.JobDescription);
				AssertEquals("JobType", "AGN", parent.JobType);
				AssertEquals("Status", "TST", parent.JobStatus);
				AssertEquals("Job description", "Shipment", parent.JobTypeDescription);
			});
		}

		public void TestCanCreateTransportBooking()
		{
			var shipment = Factory.New<AgencyShipment>();
			IDtbBookingParent parent = shipment;
			AssertEquals("CanCreateTransportBooking should always return true", true, parent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var shipment = Factory.New<AgencyShipment>();
			var dtbBookingParent = shipment as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Agency Shipment PK.", shipment.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var shipment = Factory.New<AgencyShipment>();
			var dtbBookingParent = shipment as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Agency Shipment table prefix.", shipment.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var shipment = Factory.New<AgencyShipment>();
			var dtbBookingParent = shipment as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}
	}
}
