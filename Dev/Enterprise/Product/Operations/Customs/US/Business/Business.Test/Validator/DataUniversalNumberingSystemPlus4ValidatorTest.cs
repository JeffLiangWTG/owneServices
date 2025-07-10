using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DataUniversalNumberingSystemPlus4ValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat, DataUniversalNumberingSystemPlus4Validator.Validate(""));
			AssertEquals(DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat, DataUniversalNumberingSystemPlus4Validator.Validate("123456"));
			AssertEquals(DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat, DataUniversalNumberingSystemPlus4Validator.Validate("123-12-1234"));
			AssertEquals(DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat, DataUniversalNumberingSystemPlus4Validator.Validate("123121234"));
			AssertEquals("", DataUniversalNumberingSystemPlus4Validator.Validate("1231212341234"));
		}

		//NNNNNNNNNNNNN
		public void TestIsValidDUNS()
		{
			AssertEquals(false, DataUniversalNumberingSystemPlus4Validator.IsValidDUNSPlus4(""));
			AssertEquals(false, DataUniversalNumberingSystemPlus4Validator.IsValidDUNSPlus4("061234"));
			AssertEquals(false, DataUniversalNumberingSystemPlus4Validator.IsValidDUNSPlus4("123-12-1234"));
			AssertEquals(false, DataUniversalNumberingSystemPlus4Validator.IsValidDUNSPlus4("123121234"));
			AssertEquals(true, DataUniversalNumberingSystemPlus4Validator.IsValidDUNSPlus4("1231212341234"));
		}
	}
}
