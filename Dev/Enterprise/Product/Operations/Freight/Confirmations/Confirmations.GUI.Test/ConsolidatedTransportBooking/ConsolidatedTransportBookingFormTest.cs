using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(ConsolidatedTransportBookingForm))]
	sealed class ConsolidatedTransportBookingFormTest : ZFormBasherTest
	{
		public void TestWarningMessagePrompt()
		{
			using (var form = new ConsolidatedTransportBookingForm(booking))
			{
				AssertEquals("Precondition", false, form.IsShipmentConfirmationsObsoleteWarningPrompted);
				form.Show();
				Application.DoEvents();
				AssertEquals("should have updated after message prompted", true, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			}
		}

		public void TestIsINotifications()
		{
			Assert(typeof(INotifications).IsAssignableFrom(typeof(ConsolidatedTransportBookingForm)));
		}

		protected override Form GetFormToBashCore()
		{
			return new ConsolidatedTransportBookingForm(booking);
		}

		protected override void SetUp()
		{
			booking = Factory.New<CommonConsolidatedTransportBooking>();
			base.SetUp();
		}
		CommonConsolidatedTransportBooking booking;
	}
}
