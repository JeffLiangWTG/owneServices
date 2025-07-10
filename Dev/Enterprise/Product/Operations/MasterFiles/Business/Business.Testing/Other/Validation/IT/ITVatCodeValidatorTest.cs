using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITVatCodeValidatorTest : TestCase
	{
		public void TestValidate_Pattern()
		{
			IITCusCodeValidator validator = new ITElevenDigitVATCodeValidator();
			CombineAssertions("When 11 Digit Code", () =>
			{
				AssertEquals("When Pattern Matches", ITCusCodeValidationResult.Valid, validator.Validate("00891230153"));
				AssertEquals("When Pattern do not matches", ITCusCodeValidationResult.InvalidPattern, validator.Validate("A0891230153"));
				AssertEquals("When Pattern do not matches", ITCusCodeValidationResult.InvalidPattern, validator.Validate("0089123015Q"));
			});

			validator = new ITThirteenDigitVATCodeValidator();
			CombineAssertions("When 13 Digit Code", () =>
			{
				AssertEquals("When Pattern Matches", ITCusCodeValidationResult.Valid, validator.Validate("IT00891230153"));
				AssertEquals("When Pattern do not matches", ITCusCodeValidationResult.InvalidPattern, validator.Validate("AT00891230153"));
				AssertEquals("When Pattern do not matches", ITCusCodeValidationResult.InvalidPattern, validator.Validate("IT00*91230159"));
			});
		}

		public void TestValidate_Digit()
		{
			IITCusCodeValidator validator = new ITElevenDigitVATCodeValidator();
			CombineAssertions("When 11 Digit Code", () =>
			{
				AssertEquals(ITCusCodeValidationResult.InvalidCheckDigit, validator.Validate("00891230155"));
				AssertEquals(ITCusCodeValidationResult.Valid, validator.Validate("00891230153"));
			});

			validator = new ITThirteenDigitVATCodeValidator();
			CombineAssertions("When 13 Digit Code", () =>
			{
				AssertEquals(ITCusCodeValidationResult.InvalidCheckDigit, validator.Validate("IT00891230155"));
				AssertEquals(ITCusCodeValidationResult.Valid, validator.Validate("IT00891230153"));
			});
		}
	}
}
