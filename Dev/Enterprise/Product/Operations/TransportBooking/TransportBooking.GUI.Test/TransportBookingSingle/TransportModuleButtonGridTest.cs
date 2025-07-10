using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class TransportModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			var bookingWithoutParent = Helper.CreateBooking();

			var consolidation = Helper.CreateConsolidation();
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = "JS";
			var bookingWithParent = Helper.CreateBooking(consolidation);

			var bookingWithoutConsolidationSingleJob = Factory.New<DtbBooking>();

			using (var formForBookingWithoutParent = new ZForm(bookingWithoutParent))
			{
				var bookingControl = new BookingControl();
				formForBookingWithoutParent.Controls.Add(bookingControl);
				formForBookingWithoutParent.Show();

				var orgsTab = bookingControl.OrganisationsTab;
				orgsTab.Show();

				AssertEquals(true, bookingControl.Controls.Find("BookingPartyDocumentaryAddressControl", true).Single().Visible);
			}

			using (var formForBookingWithParent = new ZForm(bookingWithParent))
			{
				var bookingControl = new BookingControl();
				formForBookingWithParent.Controls.Add(bookingControl);
				formForBookingWithParent.Show();

				var orgsTab = bookingControl.OrganisationsTab;
				orgsTab.Show();

				AssertEquals(false, bookingControl.Controls.Find("BookingPartyDocumentaryAddressControl", true).Single().Visible);
			}

			using (var formForBookingWithoutConsolidationSingleJob = new ZForm(bookingWithoutConsolidationSingleJob))
			{
				AssertEquals("ConsolidationSingleJob should be null", null, bookingWithoutConsolidationSingleJob.ConsolidationSingleJob);

				var bookingControl = new BookingControl();
				formForBookingWithoutConsolidationSingleJob.Controls.Add(bookingControl);
				formForBookingWithoutConsolidationSingleJob.Show();

				var orgsTab = bookingControl.OrganisationsTab;
				orgsTab.Show();

				AssertEquals(true, bookingControl.Controls.Find("BookingPartyDocumentaryAddressControl", true).Single().Visible);
			}
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
