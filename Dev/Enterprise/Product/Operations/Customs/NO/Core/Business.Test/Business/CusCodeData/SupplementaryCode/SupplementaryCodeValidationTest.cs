using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupplementaryCodeValidation))]
sealed class SupplementaryCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		var supplementaryCode = Factory.NewWithValidTestData<SupplementaryCode>();
		var codeInfo = supplementaryCode.CY_CodeInfo;
		CombineAssertions(() =>
		{
			supplementaryCode.Validation.ValidateCY_Code();
			AssertHasMessageErrorContaining("When CY_Code is empty", codeInfo, MandatoryValidation.YouHaveNotEntered);

			supplementaryCode.CY_Code = "1234";
			supplementaryCode.Validation.ValidateCY_Code();
			AssertNoMessageErrorContaining("When CY_Code is not empty", codeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}
}
