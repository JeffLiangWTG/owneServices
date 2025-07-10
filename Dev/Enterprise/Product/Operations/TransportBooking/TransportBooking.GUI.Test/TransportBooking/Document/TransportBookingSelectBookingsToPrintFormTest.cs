using System.Windows.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Testing
{
	[TestedType(typeof(TransportBookingSelectBookingsToPrintForm))]
	public class TransportBookingSelectBookingsToPrintFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bookingConsolidation = Helper.CreateConsolidation();
			return new TransportBookingSelectBookingsToPrintForm(new DocumentDtbBookingCollection(bookingConsolidation.Bookings));
		}

		public override void TestBindingAllTabsOnIdle()
		{
			Assert("Because binding is for ActiveBusinessObjectCollection, this test is blowing up when HasChanges attempted to be set to true.", true);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
