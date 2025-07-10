using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

internal class ExitSummarySupplementaryCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code() => CombineAssertions(() =>
	{
		supplementaryCode.CY_Code = "ABC";
		AssertHasMessageError("3 characters Supplementary Code", supplementaryCode.CY_CodeInfo, "Supplementary Code must be 4 characters long if supplied");

		supplementaryCode.CY_Code = ZString.Empty;
		AssertHasError("Empty Supplementary Code", supplementaryCode.CY_CodeInfo, "Please enter a Supplementary Code.");

		supplementaryCode.CY_Code = "ABCD";
		AssertNoNotifications("4 characters Supplementary Code", supplementaryCode.CY_CodeInfo);
	});

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	EU.Business.SupplementaryCode supplementaryCode;
}
