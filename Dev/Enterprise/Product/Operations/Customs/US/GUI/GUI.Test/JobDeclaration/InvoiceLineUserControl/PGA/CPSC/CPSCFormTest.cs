using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CPSCForm))]
	sealed class CPSCFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.Factory.Save();
			return new CPSCForm(cpsc);
		}

		public void TestRefreshWhenProcessingCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			using (var form = new CPSCForm(cpsc))
			{
				form.Show();
				AssertEquals("Certifier ID No.", form.US_ReferenceNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("Product Code Version Number", form.US_ProductIDTextBox.CaptionResourceString.Caption);
				AssertEquals("US_ProductCodeVersionNumber", form.US_ProductIDTextBox.GetBindingMember());
				AssertEquals("form.US_ProductIDTypeDropEdit.Visible", false, form.US_ProductIDTypeDropEdit.Visible);
				AssertEquals("form.US_ProductCodeTextBox.Visible", true, form.US_ProductCodeTextBox.Visible);
			}

			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			using (var form = new CPSCForm(cpsc))
			{
				form.Show();
				AssertEquals("Reference", form.US_ReferenceNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("Product ID", form.US_ProductIDTextBox.CaptionResourceString.Caption);
				AssertEquals("US_ProductID", form.US_ProductIDTextBox.GetBindingMember());
				AssertEquals("form.US_ProductIDTypeDropEdit.Visible", true, form.US_ProductIDTypeDropEdit.Visible);
				AssertEquals("form.US_ProductCodeTextBox.Visible", false, form.US_ProductCodeTextBox.Visible);
			}
		}

		public void TestLotsGridVisible()
		{
			using (var form = new CPSCForm(null))
			{
				form.Show();
				Assert(!form.LotsAndOtherSplitContainer.Panel1Collapsed);
				form.LotsGridVisible = false;
				Assert(form.LotsAndOtherSplitContainer.Panel1Collapsed);
				form.LotsGridVisible = true;
				Assert(!form.LotsAndOtherSplitContainer.Panel1Collapsed);
			}
		}
	}
}
