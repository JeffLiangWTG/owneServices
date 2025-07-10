using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(CombineBookingsForm))]
	public class CombineBookingFormTest : ZFormBasherTest
	{
		public void TestShow()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var masterBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			Factory.Save();

			var combine = new CombineBookings(masterBooking);

			using (var form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();

				AssertType(typeof(CombineBookingsForm), form);
				Assert(form.Visible);

				AssertType(typeof(CombineBookings), form.BusinessEntity);
				var combineBookings = (CombineBookings)form.BusinessEntity;

				AssertEquals(masterBooking.PK, combineBookings.MasterQuotedBooking.PK);
			}
		}

		public void TestOk()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var masterQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var otherBooking = Factory.New<ForwardingShipment>();
			var otherQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var otherQuotedBooking = QuotedBooking.New(otherQuote.PK, otherBooking.PK, Factory);

			Factory.Save();

			var combine = new CombineBookings(masterQuotedBooking);
			using (var form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();

				var otherViewQuotedBooking = ViewQuotedBooking.LoadOrCreate(otherQuotedBooking);
				combine.OtherViewQuotedBookings.Add(otherViewQuotedBooking);

				ClickOk(form);

				AssertEquals("Form should have been closed", false, form.Visible);
				AssertNotNull("LastUsedController", form.LastUsedController);
				AssertType("Should have shown the booking form", typeof(QuotedBookingForm), form.LastUsedController.LastShownForm);

				using (var bookingForm = (ZForm)form.LastUsedController.LastShownForm)
				{
					var bookingFormFactory = bookingForm.BusinessEntity.Factory;
					var cOtherBooking = bookingFormFactory.Load<ForwardingShipment>(otherBooking.PK);

					AssertEquals("cOtherBooking should be cancelled (should be combined)", true, cOtherBooking.JS_IsCancelled);
					AssertEquals("otherBooking should not be cancelled (changes should not be saved)", false, otherBooking.JS_IsCancelled);
				}
			}
		}

		public void TestOk_HandleOtherBooksErrors()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";

			var masterQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking2 = QuotedBooking.CreateNewBooking(Factory);
			booking2.JS_RL_NKOrigin = "CNSHA";
			booking2.JS_RL_NKDestination = "HKHKK";

			var otherQuotedBooking = QuotedBooking.New(quote1.PK, booking2.PK, Factory);

			Factory.Save();

			var combine = new CombineBookings(masterQuotedBooking);

			var viewOtherQuotedBooking = Factory.Load<ViewQuotedBooking>(otherQuotedBooking.PK);
			combine.OtherViewQuotedBookings.Add(viewOtherQuotedBooking);

			using (CombineBookingsForm form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				ClickOk(form);

				AssertEquals("Form should not be closed", true, form.Visible);
				AssertEquals("Please fix errors before combining.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestCancel()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var masterQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var otherBooking = Factory.New<ForwardingShipment>();
			var otherQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var otherQuotedBooking = QuotedBooking.New(otherQuote.PK, otherBooking.PK, Factory);

			Factory.Save();

			var combine = new CombineBookings(masterQuotedBooking);
			using (CombineBookingsForm form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();

				var otherViewQuotedBooking = ViewQuotedBooking.LoadOrCreate(otherQuotedBooking);
				combine.OtherViewQuotedBookings.Add(otherViewQuotedBooking);

				ClickCancel(form);

				AssertEquals("Form should have been closed", false, form.Visible);
				AssertNull("LastUsedController", form.LastUsedController);
				AssertEquals("otherBooking should not be cancelled", false, otherBooking.JS_IsCancelled);
			}
		}

		public void TestHandleZSaveConcurrencyException()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";

			var masterQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			masterQuotedBooking.TryLoadOrCreateJob();
			masterQuotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			masterQuotedBooking.Job.JH_RatingHasBeenRun = true;
			masterQuotedBooking.Mode = Core.Constants.RateMode.ULD;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			masterQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			masterQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			Factory.Save();
			Factory.RefreshEnabled = false;

			var combine = new CombineBookings(masterQuotedBooking);
			using (CombineBookingsForm form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();

				ClickOk(form);

				AssertEquals("Form should have been closed", false, form.Visible);
				AssertNotNull("LastUsedController", form.LastUsedController);
				AssertType("Should have shown the booking form", typeof(QuotedBookingForm), form.LastUsedController.LastShownForm);

				using (var quotedBookingForm = (ZForm)form.LastUsedController.LastShownForm)
				{
					var masterQuotedBookingInOpenedForm = (QuotedBooking)quotedBookingForm.BusinessEntity;
					masterQuotedBookingInOpenedForm.Factory.RefreshEnabled = false;

					masterQuotedBookingInOpenedForm.Booking.JS_RL_NKOrigin = "USALX";
					masterQuotedBookingInOpenedForm.Booking.JS_RL_NKDestination = "NLAMS";

					booking.JS_RL_NKOrigin = "AUBNE";
					booking.JS_RL_NKDestination = "CNNJG";
					Factory.Save();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					quotedBookingForm.FireSaveButton();

					AssertEquals("While you have been working with this form, another user has made changes which cannot be merged. This form would be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var masterQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			masterQuotedBooking.Mode = Core.Constants.RateMode.LCL;

			Factory.Save();

			var combine = new CombineBookings(masterQuotedBooking);

			return new CombineBookingsForm(combine);
		}

		void ClickOk(CombineBookingsForm form)
		{
			ClickButton(form, "okButton");
		}

		void ClickCancel(CombineBookingsForm form)
		{
			ClickButton(form, "cancelButton");
		}

		void ClickButton(CombineBookingsForm form, string buttonName)
		{
			Control[] foundControls = form.Controls.Find(buttonName, true);
			if (foundControls.Length != 1)
			{
				throw new Exception("Argh!");
			}

			Button button = (Button)foundControls[0];
			button.PerformClick();
		}

		#endregion
	}
}
