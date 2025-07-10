using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITFiscalCodeValidatorTest : TestCase
	{
		public void TestValidate()
		{
			var validator = CreateFiscalCodeValidator();
			CombineAssertions(() =>
			{
				var code = "ABCDEF12B01Q123A";
				AssertEquals("When Code is matching the pattern", ITCusCodeValidationResult.Valid, validator.Validate(code));

				code = "A0089123B15A123Q";
				AssertEquals($"When Code ${code} is not matching the pattern", ITCusCodeValidationResult.InvalidPattern, validator.Validate(code));

				code = "0A089123B15A1_3Q";
				AssertEquals($"When Code ${code} is not matching the pattern", ITCusCodeValidationResult.InvalidPattern, validator.Validate(code));
			});
		}

		IITCusCodeValidator CreateFiscalCodeValidator() => new ITFiscalCodeValidator();
	}
}
