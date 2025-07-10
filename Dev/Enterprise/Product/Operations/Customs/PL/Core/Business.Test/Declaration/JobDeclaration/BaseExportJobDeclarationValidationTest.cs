using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

abstract class BaseExportJobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
{
	public void TestCheckJE_UCR_RuleC0059() => CombineAssertions(() =>
	{
		const string notRequiredErrorMessage = "[C0059] DUCR number should be empty when Circumstance contains value 'A20'.";
		const string requiredErrorMessage = "[C0059] DUCR number is required when Circumstance is empty.";

		jobDeclaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
		jobDeclaration.JE_UCR = "1234567890";
		AssertHasMessageError("Specific Circumstance Indicator: A20, UCR: non-empty", jobDeclaration.JE_UCRInfo, notRequiredErrorMessage);
		jobDeclaration.JE_UCR = ZString.Empty;
		AssertNoMessageError("Specific Circumstance Indicator: A20, UCR: empty", jobDeclaration.JE_UCRInfo, notRequiredErrorMessage);

		jobDeclaration.ZG_SpecificCircumstanceIndicator = ZString.Empty;
		jobDeclaration.JE_UCR = "1234567890";
		AssertNoMessageError("Specific Circumstance Indicator: empty, UCR: non-empty", jobDeclaration.JE_UCRInfo, requiredErrorMessage);
		jobDeclaration.JE_UCR = ZString.Empty;
		AssertHasMessageError("Specific Circumstance Indicator: empty, UCR: empty", jobDeclaration.JE_UCRInfo, requiredErrorMessage);
	});

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
	}

	protected JobDeclaration jobDeclaration;
}
