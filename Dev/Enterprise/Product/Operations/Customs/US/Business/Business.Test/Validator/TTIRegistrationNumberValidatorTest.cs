using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TTIRegistrationNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat, TTIRegistrationNumberValidator.Validate(""));
			AssertEquals(TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat, TTIRegistrationNumberValidator.Validate("AAAA"));
			AssertEquals(TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat, TTIRegistrationNumberValidator.Validate("AAA-W23"));
			AssertEquals(TTIRegistrationNumberValidator.TTIRegistrationNumberMaxLength, TTIRegistrationNumberValidator.Validate("DSP-CA-9021012345"));
			AssertEquals("", TTIRegistrationNumberValidator.Validate("DSP-CA-90210"));
			AssertEquals("", TTIRegistrationNumberValidator.Validate("BR-MA-SAM-1"));
			AssertEquals("", TTIRegistrationNumberValidator.Validate("BR-MA-SAM-DER2#"));
		}
	}
}
