using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class AdditionalProcedureCodeValidationTest : CusCodeDataValidationTest
{
	public void TestCheckCY_Code_RuleB0001E() => CombineAssertions(() =>
	{
		const string ruleB0001E_ErrorMessage = "[B0001E] \"1H2\" Additional Procedure code is not allowed during the transition period.";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine.JI_CEI = entryInstruction.PK;
		var additionalProcedureCode = invLine.AdditionalProcedureCodes.AddNew();

		using var testContext = new FunctionalityTestContext();
		testContext.SetAESTransitionPeriod(effectiveDate: entryInstruction.CEI_DateForDuty, enabled: true);

		additionalProcedureCode.CY_Code = "2222222";
		AssertNoMessageError($"Inside the transition period, {additionalProcedureCode.CY_Code}", additionalProcedureCode.CY_CodeInfo, ruleB0001E_ErrorMessage);

		additionalProcedureCode.CY_Code = "1H2";
		AssertHasMessageError($"Inside the transition period, {additionalProcedureCode.CY_Code}", additionalProcedureCode.CY_CodeInfo, ruleB0001E_ErrorMessage);

		testContext.SetAESTransitionPeriod(effectiveDate: entryInstruction.CEI_DateForDuty, enabled: false);
		additionalProcedureCode.Validation.ValidateCY_Code();
		AssertNoMessageError($"Outside the transition period, {additionalProcedureCode.CY_Code}", additionalProcedureCode.CY_CodeInfo, ruleB0001E_ErrorMessage);
	});
}
