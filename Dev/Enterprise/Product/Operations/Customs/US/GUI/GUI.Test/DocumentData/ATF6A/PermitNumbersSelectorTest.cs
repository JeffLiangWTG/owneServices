using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class PermitNumbersSelectorTest : TestCaseWithFactory
	{
		public void TestSelectPermitNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceline = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceline.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atfLine = invoiceline.ATFLines.AddNew();
			atfLine.US_PermitNumber = "123";
			atfLine = invoiceline.ATFLines.AddNew();
			atfLine.US_PermitNumber = "456";

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			var permitNumbersSelector = new PermitNumbersSelector();
			var selectedPermitNumbers = permitNumbersSelector.SelectPermitNumbers(declaration);
			Assert(selectedPermitNumbers.IsRight);
			AssertContainsExactElementsInAnyOrder("selected permit numbers", new[] { "123", "456" }, selectedPermitNumbers.Right);
			var form = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<PermitNumbersSelectorForm>(form);
		}
	}
}
