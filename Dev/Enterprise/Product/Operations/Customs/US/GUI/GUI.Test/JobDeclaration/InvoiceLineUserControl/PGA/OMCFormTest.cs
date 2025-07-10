using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(OMCForm))]
	sealed class OMCFormTest : ZFormBasherTest
	{
		public void TestControlLabelName()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var control = form.Controls.Find("US_SourceCountryCodeFindBox", true)[0] as IResCaptionedControl;
				AssertEquals("US_SourceCountryCodeFindBox label", "Source Country/Region", control.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var omc = invoiceLine.OMCHeaders.AddNew();
			omc.Factory.Save();
			return new OMCForm(omc);
		}
	}
}
