using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CommonBookingInformationFormTest : TestCaseWithFactory
	{
		public void TestSetControlsVisibility()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			Factory.Save();
			CommonCartageBookingInformation bookingInformation = cartage.BookingInformation;
			using (CommonBookingInformationForm bookingInfoForm = new CommonBookingInformationForm(bookingInformation))
			{
				bookingInfoForm.Show();
				AssertEquals("Precondition:", false, bookingInformation.CanSelectStatus);
				AssertEquals("Precondition:", true, bookingInformation.CanExposeBookingAction);
				AssertEquals("BookingStatusLabel", false, bookingInfoForm.BookingStatusLabel.Visible);
				AssertEquals("BookingStatusDropEdit", false, bookingInfoForm.BookingStatusDropEdit.Visible);
				AssertEquals("BookingGroupBox", true, bookingInfoForm.BookingGroupBox.Enabled);
				AssertEquals("BookingGroupBox", true, bookingInfoForm.BookingGroupBox.Visible);
			}
		}
		public void TestMessageShownWhenCartageHasErrors()
		{
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();
			var bookingInformation = new CommonCartageBookingInformation(cartage);
			using (var bookingInfoForm = new CommonBookingInformationForm(bookingInformation))
			{
				bookingInfoForm.Show();
				// Set up validation error
				cartage.Logs.AddNew(AutoEvents.StatusUpdated, FreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted + "-" +
				FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted); // For CanSelectStatus check
				AssertEquals("Precondition: Can select status", true, bookingInformation.CanSelectStatus);
				bookingInformation.BookingStatus = "ABC"; // POKE
				AssertHasError("Precondition", bookingInformation.BookingStatusInfo, "Enter a valid selection.");
				bookingInfoForm.OKButton.PerformClick();
				Assert("Should have shown message for fixing errors", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Please fix the errors before continuing.") > -1);
			}
		}
	}

	[TestedType(typeof(CommonBookingInformationForm))]
	public class CommonBookingInformationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonCartageBookingInformation bookingInformation = cartage.BookingInformation;
			return new CommonBookingInformationForm(bookingInformation);
		}
	}
}
