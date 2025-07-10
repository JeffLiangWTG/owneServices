using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(VAT404DocumentSendingForm))]
	sealed class VAT404DocumentSendingFormTest : ZFormBasherTest
	{
		public void TestVisibility()
		{
			VAT404TestHelper.SetupPayInfo(Factory);
			var tester = new VAT404DocumentInstruction(Factory);
			using (var form = new VAT404DocumentSendingForm(tester))
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("BackButton", true)[0].Visible);
				AssertEquals(false, form.Controls.Find("SendButton", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("SearchButton", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("CancelButton", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("vaT404FilterUserControl1", true)[0].Visible);
				AssertEquals(false, form.Controls.Find("vaT404DocumentUserControl1", true)[0].Visible);
				(form.Controls.Find("SearchButton", true)[0] as ZButton).PerformClick();
				AssertEquals(true, form.Controls.Find("BackButton", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("SendButton", true)[0].Visible);
				AssertEquals(false, form.Controls.Find("SearchButton", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("CancelButton", true)[0].Visible);
				AssertEquals(false, form.Controls.Find("vaT404FilterUserControl1", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("vaT404DocumentUserControl1", true)[0].Visible);
			}
		}

		protected override Form GetFormToBashCore() => new VAT404DocumentSendingForm(new VAT404DocumentInstruction(Factory));
	}
}
