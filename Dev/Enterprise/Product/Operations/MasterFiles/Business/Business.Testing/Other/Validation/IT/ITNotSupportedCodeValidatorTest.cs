using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITNotSupportedCodeValidatorTest : TestCase
	{
		public void TestValidate()
		{
			var validator = new ITNotSupportedCodeValidator();
			AssertEquals(ITCusCodeValidationResult.InvalidLength, validator.Validate(""));
		}
	}
}
