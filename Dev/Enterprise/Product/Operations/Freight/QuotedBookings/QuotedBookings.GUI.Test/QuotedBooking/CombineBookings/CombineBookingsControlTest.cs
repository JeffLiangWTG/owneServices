using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class CombineBookingsControlTest : TestCaseWithFactory
	{
		public void TestVoyageNumberTextBox()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var combine = new CombineBookings(quotedBooking);

			using (var form = new ZForm())
			using (var control = new CombineBookingsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				control.SetDataBinding(combine, "");
				Assert(control.VoyageNumberTextBox.Visible);
				AssertEquals("Flight No", control.VoyageNumberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				Assert(!control.VesselTextBox.Visible);
				AssertEquals("Vessel", control.VesselTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				quotedBooking.Mode = Core.Constants.RateMode.LCL;
				control.SetDataBinding(combine, "");
				Assert(control.VoyageNumberTextBox.Visible);
				AssertEquals("Voyage No", control.VoyageNumberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				Assert(control.VesselTextBox.Visible);
				AssertEquals("Vessel", control.VesselTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				quotedBooking.Mode = Core.Constants.RateMode.LRA;
				control.SetDataBinding(combine, "");
				Assert(control.VoyageNumberTextBox.Visible);
				AssertEquals("Journey", control.VoyageNumberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				Assert(control.VesselTextBox.Visible);
				AssertEquals("Vessel", control.VesselTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				quotedBooking.Mode = Core.Constants.RateMode.LRO;
				control.SetDataBinding(combine, "");
				Assert(control.VoyageNumberTextBox.Visible);
				AssertEquals("Truck Ref.", control.VoyageNumberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				Assert(!control.VesselTextBox.Visible);
				AssertEquals("Vessel", control.VesselTextBox.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestRemoveActionIsRemoveOnly()
		{
			using (var control = new CombineBookingsControlForTest())
			{
				control.Show();
				AssertEquals("Remove action must be Remove Only", RemoveAction.Remove, control.BookingsModuleButtonGrid.InnerGrid.RemoveAction);
			}
		}
	}
}
