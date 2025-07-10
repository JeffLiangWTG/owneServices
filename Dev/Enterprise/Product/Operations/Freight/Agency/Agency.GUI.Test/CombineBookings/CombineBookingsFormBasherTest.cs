using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(CombineBookingsForm))]
	internal class CombineBookingsFormBasherTest : ZFormBasherTest
	{
		public void TestShow()
		{
			AgencyBooking masterBooking = Factory.New<AgencyBooking>();
			Factory.Save();
			using (CombineBookingsForm form = CombineBookingsForm.Show(masterBooking))
			{
				Application.DoEvents();
				AssertType(typeof(CombineBookingsForm), form);
				AssertEquals(true, form.Visible);
				AssertType(typeof(CombineBookings), form.BusinessEntity);
				CombineBookings combineBookings = (CombineBookings)form.BusinessEntity;
				AssertEquals(masterBooking.PK, combineBookings.MasterBooking.PK);
			}
		}

		public void TestOk()
		{
			AgencyBooking masterBooking = Factory.New<AgencyBooking>();
			AgencyBooking otherBooking = Factory.New<AgencyBooking>();
			Factory.Save();
			CombineBookings combine = new CombineBookings(masterBooking);
			using (CombineBookingsForm form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();
				combine.OtherBookings.Add(otherBooking);
				ClickOk(form);
				Application.DoEvents();
				AssertEquals("Form should have been closed", false, form.Visible);
				AssertNotNull("LastUsedController", form.LastUsedController);
				AssertType("Should have shown the booking form", typeof(AgencyBookingForm), form.LastUsedController.LastShownForm);
				using (ZForm bookingForm = (ZForm)form.LastUsedController.LastShownForm)
				{
					BusinessObjectFactory bookingFormFactory = bookingForm.BusinessEntity.Factory;
					AgencyBooking cOtherBooking = bookingFormFactory.Load<AgencyBooking>(otherBooking.PK);
					AssertEquals("cOtherBooking should be cancelled (should be combined)", true, cOtherBooking.JS_IsCancelled);
					AssertEquals("otherBooking should not be cancelled (changes should not be saved)", false, otherBooking.JS_IsCancelled);
				}
			}
		}

		public void TestCancell()
		{
			AgencyBooking masterBooking = Factory.New<AgencyBooking>();
			AgencyBooking otherBooking = Factory.New<AgencyBooking>();
			Factory.Save();
			CombineBookings combine = new CombineBookings(masterBooking);
			using (CombineBookingsForm form = new CombineBookingsForm(combine))
			{
				form.Show();
				Application.DoEvents();
				combine.OtherBookings.Add(otherBooking);
				ClickCancel(form);
				Application.DoEvents();
				AssertEquals("Form should have been closed", false, form.Visible);
				AssertNull("LastUsedController", form.LastUsedController);
				AssertEquals("otherBooking should not be cancelled", false, otherBooking.JS_IsCancelled);
			}
		}

		#region Implementation
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

		protected override void SetUp()
		{
			base.SetUp();
			CombineBookingsForm.SuppressAutoCloseInTests = true;
		}

		protected override void TearDown()
		{
			CombineBookingsForm.SuppressAutoCloseInTests = false;
			base.TearDown();
		}

		protected override Form GetFormToBashCore()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			Factory.Save();
			CombineBookings combine = new CombineBookings(booking);
			return new CombineBookingsForm(combine);
		}
		#endregion
	}
}
