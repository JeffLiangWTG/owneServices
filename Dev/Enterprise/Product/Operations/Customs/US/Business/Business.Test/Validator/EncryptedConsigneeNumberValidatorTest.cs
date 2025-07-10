using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EncryptedConsigneeNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat, EncryptedConsigneeNumberValidator.Validate(""));
			AssertEquals(EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat, EncryptedConsigneeNumberValidator.Validate("-123456"));
			AssertEquals(EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat, EncryptedConsigneeNumberValidator.Validate("-123 12-1234"));
			AssertEquals("", EncryptedConsigneeNumberValidator.Validate("-123a2B12-34"));
		}

		//-CCCCCCCCCCC
		public void TestIsValidDUNS()
		{
			AssertEquals(false, EncryptedConsigneeNumberValidator.IsValidEncryptedNumber(""));
			AssertEquals(false, EncryptedConsigneeNumberValidator.IsValidEncryptedNumber("061234"));
			AssertEquals(false, EncryptedConsigneeNumberValidator.IsValidEncryptedNumber("-123-12 1233"));
			AssertEquals(true, EncryptedConsigneeNumberValidator.IsValidEncryptedNumber("-12A12CD34-1"));
		}
	}
}
