using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ErrorCodeTest : TestCase
	{
		public void TestConstructor()
		{
			ErrorCode ec = new ErrorCode("Status", "ErrorCode", "Description");
			AssertEquals("ErrorCode", ec.Code);
			AssertEquals("Description", ec.Description);
			AssertEquals("Status", ec.StatusCode);
		}
	}

	sealed class ErrorCodeListTest : TestCase
	{
		public void TestItemCount()
		{
			AssertEquals("Sars EDI User Manual entry count", 16, new ErrorCodeList().Count);
		}
	}
}
