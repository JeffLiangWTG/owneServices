using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(InvoiceImporterForm))]
	sealed class InvoiceImporterFormTest : ZFormBasherTest
	{
		public void TestInstantiateForm()
		{
			using (InvoiceImporterForm form = new InvoiceImporterForm())
			{
				form.Show();
				AssertEquals(false, form.SkipUnknownSupplierRecords);
				AssertEquals("Form.Text", "Invoice Data Importer", form.Text);
				Assert("Form.ControlBox, display minimize/maximize/close buttons on right of title bar", form.ControlBox);

				var processButton = (ZButton)form.Controls.Find("ProcessButton", true).Single();
				AssertEquals("ProcessButton.Anchor", AnchorStyles.Top | AnchorStyles.Right, processButton.Anchor);
			}
		}

		protected override Form GetFormToBashCore() => new InvoiceImporterForm();
	}
}
