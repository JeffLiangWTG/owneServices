using System.Linq;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVBookingHeaderCreateTestLoadListForm))]
	public class HVLVBookingHeaderCreateTestLoadListFormTest : ZFormBasherTest
	{
		public void TestCreateTestLoadListForm_ShouldCloseForm_WhenButtonOKClick()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderCreateTestLoadListForm(bookingHeader))
			{
				form.Show();

				var bookingHeaderReference = form.Controls.Find("textBoxBookingHeaderReference", true).SingleOrDefault();
				AssertNotNull(bookingHeaderReference);

				var okBtn = form.Controls.Find("btnOK", true).SingleOrDefault() as ZButton;
				AssertNotNull(okBtn);

				okBtn.PerformClick();
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestCreateTestLoadListForm_ShouldCloseForm_WhenKeyDownEsacpe()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderCreateTestLoadListForm(bookingHeader))
			{
				form.Show();

				var bookingHeaderReference = form.Controls.Find("textBoxBookingHeaderReference", true).SingleOrDefault();
				AssertNotNull(bookingHeaderReference);
				KeySender.PostKeyDown(form, Keys.Escape);
				Application.DoEvents();

				AssertEquals(true, form.IsDisposed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new HVLVBookingHeaderCreateTestLoadListForm(Factory.NewWithValidTestData<HVLVBookingHeader>());
		}
	}
}
