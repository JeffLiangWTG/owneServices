using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class SouthAfricanOrganisationCodeValidatorTest : TestCase
	{
		public void TestIsValidWithCCDSCSNumbers()
		{
			AssertEquals(false, validator.IsValid("10000097")); //6
			AssertEquals(false, validator.IsValid("10000245")); //7
			AssertEquals(true, validator.IsValid("10000096"));
			AssertEquals(true, validator.IsValid("10000247"));
			AssertEquals(true, validator.IsValid("10000029")); //9
			AssertEquals(false, validator.IsValid("92000102")); //0
			AssertEquals(false, validator.IsValid("93102006")); //1
		}

		public void TestValidShortCode()
		{
			AssertEquals("Short Number Valid", true, validator.IsValid(" 281124 "));
		}

		public void TestValidLongCode()
		{
			AssertEquals("Long Number Valid", true, validator.IsValid("20265566"));
		}

		public void TestInvalidCode()
		{
			Assert("Validation code did not detect invalid code", !validator.IsValid("281125"));
		}

		public void TestInvalidTextCode()
		{
			Assert("Validation code did not detect invalid code", !validator.IsValid("Blah"));
		}

		public void TestInvalidZeroCode()
		{
			Assert("Validation code did not detect invalid code", !validator.IsValid("0"));
		}

		SouthAfricanOrganisationCodeValidator validator;
		protected override void SetUp()
		{
			base.SetUp();
			validator = new SouthAfricanOrganisationCodeValidator();
		}
	}
}
