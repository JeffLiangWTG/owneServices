using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing
{
	class ECCNCodesUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new ECCNCodesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Host", control, control.Host);
					AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
				});
			}
		}

		public void TestZLabelCaptionRenderer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new ECCNCodesUserControl())
			{
				form.CaptionRenderingEnabled = true;
				form.Controls.Add(control);
				form.Show();

				AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
			}
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new ECCNCodesUserControl())
			{
				AssertEquals(nameof(JobComInvoiceLine.ECCNCodesAsString), control.ResourceStringBindingMember);
			}
		}

		public void TestECCNCodesEditButton_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new ECCNCodesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;

				var button = control.FindSingle<ZButton>("ECCNCodesEditButton");
				button.PerformClick();
				AssertType<ECCNCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
