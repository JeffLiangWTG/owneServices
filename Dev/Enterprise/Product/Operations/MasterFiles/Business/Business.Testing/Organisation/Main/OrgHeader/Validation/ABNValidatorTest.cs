using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ABNValidatorTest : TestCase
	{
		public void TestValidABN()
		{
			AssertEquals("Valid ABN", true, ABNValidation.CheckValidABN("21 003 980 130"));
		}

		public void TestValidABNWithCAC()
		{
			AssertEquals("Valid ABN", true, ABNValidation.CheckValidABN("21 003 980 130 123"));
		}

		public void TestInvalidABN()
		{
			AssertEquals("Invalid ABN", false, ABNValidation.CheckValidABN("21 003 980 131"));
		}

		public void TestInvalidABNWithCAC()
		{
			AssertEquals("Valid ABN", false, ABNValidation.CheckValidABN("21 003 980 131 123"));
		}

		public void TestInvalidLengthABN()
		{
			AssertEquals("Valid ABN", false, ABNValidation.CheckValidABN("21 003 980 130 01"));
		}

		public void TestEmptyABN()
		{
			AssertEquals("Empty ABN", false, ABNValidation.CheckValidABN(""));
		}
	}
}
