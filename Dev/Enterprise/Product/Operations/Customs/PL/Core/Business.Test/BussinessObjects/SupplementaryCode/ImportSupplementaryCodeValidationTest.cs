using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class ImportSupplementaryCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		var messageError = "(R1045) Additional Sup.Codes are not allowed with procedure details code 2PL.";
		var concessionCode = invoiceLine.AdditionalProcedureCodes.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty SupplementaryCode", supplementaryCode.CY_CodeInfo, messageError);

			supplementaryCode.CY_Code = "asd";
			AssertNoMessageError("Not empty SupplementaryCode", supplementaryCode.CY_CodeInfo, messageError);

			concessionCode.CY_Code = Constants.ConcessionCodes.C10;
			supplementaryCode.Validation.ValidateCY_Code();
			AssertNoMessageError("Concession Code is not 2PL", supplementaryCode.CY_CodeInfo, messageError);

			concessionCode.CY_Code = Constants.ConcessionCodes._2PL;
			supplementaryCode.Validation.ValidateCY_Code();
			AssertHasMessageError("Concession Code is 2PL", supplementaryCode.CY_CodeInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	EU.Business.SupplementaryCode supplementaryCode;
}
