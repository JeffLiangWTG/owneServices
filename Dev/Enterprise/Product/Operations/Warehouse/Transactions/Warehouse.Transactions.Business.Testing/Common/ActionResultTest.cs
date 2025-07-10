using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ActionResultTest : TestCase
	{
		public void TestSuccess()
		{
			var result = ActionResult.Success();
			Assert(result.IsSuccess);
			Assert(string.IsNullOrEmpty(result.ErrorMessage));
		}

		public void TestFailure()
		{
			var result = ActionResult.Failure("Bla");
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Bla", result.ErrorMessage);
		}

		public void TestFailure_MustNotPassEmptyErrorMessage()
		{
			AssertExceptionThrown<ArgumentException>(() => ActionResult.Failure(""));
		}

		public void TestFailure_MustNotPassNullErrorMessage()
		{
			AssertExceptionThrown<ArgumentException>(() => ActionResult.Failure(null));
		}
	}
}
