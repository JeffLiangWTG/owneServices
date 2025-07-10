using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing;
class SupplementaryCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_CodeExport()
	{
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var supplementaryCode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode1.CY_Code = "SUPCODE1";
		AssertNoMessageError(supplementaryCode1.CY_CodeInfo, SupplementaryCodeValidation.EXPCodesCount(supplementaryCode1.CY_CodeInfo));
		var supplementaryCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode2.CY_Code = "SUPCODE2";
		AssertNoMessageError(supplementaryCode2.CY_CodeInfo, SupplementaryCodeValidation.EXPCodesCount(supplementaryCode2.CY_CodeInfo));
		var supplementaryCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode3.CY_Code = "SUPCODE3";
		AssertNoMessageError(supplementaryCode3.CY_CodeInfo, SupplementaryCodeValidation.EXPCodesCount(supplementaryCode3.CY_CodeInfo));
		var supplementaryCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode4.CY_Code = "SUPCODE4";
		AssertHasMessageError(supplementaryCode4.CY_CodeInfo, SupplementaryCodeValidation.EXPCodesCount(supplementaryCode4.CY_CodeInfo));
	}

	public void TestCheckCY_CodeImport()
	{
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var supplementaryCode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode1.CY_Code = "SUPCODE1";
		AssertNoMessageError(supplementaryCode1.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode1.CY_CodeInfo));
		var supplementaryCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode2.CY_Code = "SUPCODE2";
		AssertNoMessageError(supplementaryCode2.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode2.CY_CodeInfo));
		var supplementaryCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode3.CY_Code = "SUPCODE3";
		AssertNoMessageError(supplementaryCode3.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode3.CY_CodeInfo));
		var supplementaryCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode4.CY_Code = "SUPCODE4";
		AssertNoMessageError(supplementaryCode4.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode4.CY_CodeInfo));
		var supplementaryCode5 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode5.CY_Code = "SUPCODE5";
		AssertNoMessageError(supplementaryCode5.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode5.CY_CodeInfo));
		var supplementaryCode6 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode6.CY_Code = "SUPCODE6";
		AssertHasMessageError(supplementaryCode6.CY_CodeInfo, SupplementaryCodeValidation.IMPCodesCount(supplementaryCode6.CY_CodeInfo));
	}

	public void TestDuplication()
	{
		var supplementaryCode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode1.CY_Code = "SUPCODE1";
		var supplementaryCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode2.CY_Code = "SUPCODE2";
		AssertNoMessageError(supplementaryCode2.CY_CodeInfo, SupplementaryCodeValidation.DuplicateSupplementaryCode(supplementaryCode2.CY_CodeInfo));
		var supplementaryCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode3.CY_Code = "SUPCODE1";
		AssertHasMessageError(supplementaryCode3.CY_CodeInfo, SupplementaryCodeValidation.DuplicateSupplementaryCode(supplementaryCode3.CY_CodeInfo));
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
}
