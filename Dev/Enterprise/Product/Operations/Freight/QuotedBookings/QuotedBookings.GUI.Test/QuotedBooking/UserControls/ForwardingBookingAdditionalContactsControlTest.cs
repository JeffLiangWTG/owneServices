using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class ForwardingBookingAdditionalContactsControlTest : TestCaseWithFactory
	{
		public void TestReceiverDeliveryLabel()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			using (var form = new ZForm(quotedBooking))
			using (var forwarderAdditionalContactsControl = new ForwardingBookingAdditionalContactsControl())
			{
				form.Controls.Add(forwarderAdditionalContactsControl);
				form.Show();
				var receivalPointGroupBox = forwarderAdditionalContactsControl.Controls.Find("ReceivalPointGroupBox", true).FirstOrDefault() as ZGroupBox;
				var deliveryPointGroupBox = forwarderAdditionalContactsControl.Controls.Find("DeliveryPointGroupBox", true).FirstOrDefault() as ZGroupBox;
				AssertEquals("Pickup CFS", receivalPointGroupBox.Text);
				AssertEquals("Delivery CFS", deliveryPointGroupBox.Text);
				quotedBooking.Mode = "LSE";
				AssertEquals("Pickup CFS", receivalPointGroupBox.Text);
				AssertEquals("Delivery CFS", deliveryPointGroupBox.Text);
				quotedBooking.Mode = "FCL";
				AssertEquals("Pickup CTO", receivalPointGroupBox.Text);
				AssertEquals("Delivery CTO", deliveryPointGroupBox.Text);
			}
		}
	}
}
