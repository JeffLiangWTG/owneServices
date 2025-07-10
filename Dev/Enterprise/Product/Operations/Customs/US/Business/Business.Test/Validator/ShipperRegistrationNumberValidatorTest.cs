using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ShipperRegistrationNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(ShipperRegistrationNumberValidator.SFRRightFormat, ShipperRegistrationNumberValidator.Validate(""));
			AssertEquals(ShipperRegistrationNumberValidator.SFRRightFormat, ShipperRegistrationNumberValidator.Validate("061234"));
			AssertEquals("", ShipperRegistrationNumberValidator.Validate("34545632145"));
			AssertEquals(ShipperRegistrationNumberValidator.SFRRightFormat, ShipperRegistrationNumberValidator.Validate("3A512365245"));
			AssertEquals(ShipperRegistrationNumberValidator.SFRRightFormat, ShipperRegistrationNumberValidator.Validate("3 545632145"));
		}

		public void TestIsValidSFRCode()
		{
			AssertEquals(false, ShipperRegistrationNumberValidator.IsValidSFR(""));
			AssertEquals(false, ShipperRegistrationNumberValidator.IsValidSFR("061234"));
			AssertEquals(true, ShipperRegistrationNumberValidator.IsValidSFR("34545632514"));
			AssertEquals(false, ShipperRegistrationNumberValidator.IsValidSFR("3 545632154"));
			AssertEquals(false, ShipperRegistrationNumberValidator.IsValidSFR("3A545632514"));
		}
	}
}
