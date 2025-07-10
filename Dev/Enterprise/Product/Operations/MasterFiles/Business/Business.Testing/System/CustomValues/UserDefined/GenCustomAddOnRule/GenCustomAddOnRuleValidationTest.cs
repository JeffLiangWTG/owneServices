using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GenCustomAddOnRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCodeValidation()
		{
			var rule0 = Factory.New<GenCustomAddOnRule>();
			rule0.XR_Code = "Y";

			var rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_Code = "Y";
			AssertHasError(rule.XR_CodeInfo, "The specified rule code 'Y' already exists. Please enter a unique value.");
			rule.XR_Code = "X";
			AssertNoErrors(rule.XR_CodeInfo);
			rule.XR_Code = "";
			AssertHasError(rule.XR_CodeInfo, "Please enter a Code.");
		}
	}
}
