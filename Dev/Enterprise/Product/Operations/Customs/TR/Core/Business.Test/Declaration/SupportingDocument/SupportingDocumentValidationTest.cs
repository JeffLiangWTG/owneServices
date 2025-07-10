using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class SupportingDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_Status()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocumentForLine.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Status = "X";
				supportingDocumentForLine.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("Should have InvalidCodeMessageError", supportingDocumentForLine.CSI_StatusInfo, ListValidation.InvalidCodeMessageError.ToString());
				supportingDocumentForLine.CSI_Status = SupportingDocumentAvailabilityList.Codes.B;
				AssertNoMessageErrorContaining("Should not have message", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}
}
