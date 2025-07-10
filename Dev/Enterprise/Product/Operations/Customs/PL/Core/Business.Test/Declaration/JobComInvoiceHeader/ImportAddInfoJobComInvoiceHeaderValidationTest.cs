using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_AgreedPlaceCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var targetInfo = invoice.ZG_AgreedPlaceCodeInfo;

		CombineAssertions(() =>
		{
			invoice.ZG_AgreedPlaceCode = ZString.Empty;
			AssertHasMessageErrorContaining("Not Entered", targetInfo, "Incoterm Place Code or Country Code is required");

			invoice.ZG_AgreedPlaceCode = CountryCodes.Poland;
			AssertNoNotifications("Entered", targetInfo);

			invoice.ZG_AgreedPlaceCode = "1";
			AssertHasMessageErrorContaining("Invalid entry", targetInfo, "Incoterm Place Code must either be a valid country or a valid UNLOCODE");
		});
	}

	public void TestCheckRuleR284()
	{
		var messageError = "(R284) You have not entered a Valuation Method.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			invoice.ZG_ValuationMethod = ZString.Empty;
			AssertHasMessageError("Empty Setup", invoice.ZG_ValuationMethodInfo, messageError);

			var concessionCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			concessionCode.CY_Code = Constants.ConcessionCodes._2PL;
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 31", invoice.ZG_ValuationMethodInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 71", invoice.ZG_ValuationMethodInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 76", invoice.ZG_ValuationMethodInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			concessionCode.CY_Code = Constants.ConcessionCodes.C20;
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertHasMessageError("Concession Code is not 2PL with procedure code 71/76 not specified", invoice.ZG_ValuationMethodInfo, messageError);

			concessionCode.CY_Code = Constants.ConcessionCodes._2PL;
			invoice.ZG_ValuationMethod = "A";
			AssertNoMessageError("ZG_ValuationMethod is not empty", invoice.ZG_ValuationMethodInfo, messageError);
		});
	}

	public void TestZG_ValuationMethod_ListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();

		invoiceHeader.ZG_ValuationMethod = "A";
		AssertHasMessageError(invoiceHeader.ZG_ValuationMethodInfo, ListValidation.InvalidCodeMessageError);
	}
}
