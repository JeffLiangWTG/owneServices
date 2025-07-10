using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVBookingHeaderCreateTestConsignmentForm))]
	public class HVLVBookingHeaderCreateTestConsignmentFormTest : ZFormBasherTest
	{
		public void TestCreateTestConsignmentForm_ShouldCloseFormAndConsignmentsCreate_WhenButtonOKClick()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			factory.Save();

			using (var form = new HVLVBookingHeaderCreateTestConsignmentForm(bookingHeader.HVH_BookingReference, bookingHeader.Consignments.Any()))
			{
				form.Show();
				var okBtn = form.Controls.Find("btnOK", true).SingleOrDefault() as ZButton;
				AssertNotNull(okBtn);

				var consignmentDataSetting = form.Controls.Find("calcEditConsignmentCount", true).SingleOrDefault() as ZCalcEdit;
				AssertNotNull(consignmentDataSetting);
				consignmentDataSetting.CalcValue = 5;

				okBtn.PerformClick();

				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestCreateTestConsignmentForm_ShouldCloseFormAndNoDataCreate_WhenKeyDownEsacpe()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderCreateTestConsignmentForm(bookingHeader.HVH_BookingReference, bookingHeader.Consignments.Any()))
			{
				form.Show();
				var okBtn = form.Controls.Find("btnOK", true).SingleOrDefault() as ZButton;
				AssertNotNull(okBtn);

				var consignmentDataSetting = form.Controls.Find("calcEditConsignmentCount", true).SingleOrDefault() as ZCalcEdit;
				AssertNotNull(consignmentDataSetting);
				consignmentDataSetting.CalcValue = 5;

				KeySender.PostKeyDown(form, Keys.Escape);
				Application.DoEvents();

				AssertEquals(true, form.IsDisposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new HVLVBookingHeaderCreateTestConsignmentForm("", false);
		}
	}
}
