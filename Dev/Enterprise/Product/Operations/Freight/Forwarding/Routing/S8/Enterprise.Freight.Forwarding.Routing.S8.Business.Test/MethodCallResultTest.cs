using System;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class MethodCallResultTest : TestCase
	{
		public void TestMethodCallResultWithValidResult()
		{
			var methodCallResult = new MethodCallResult<int>(3);
			AssertEquals(true, methodCallResult.Succeeded);
			AssertEquals(3, methodCallResult.Result);
			AssertExceptionThrown(typeof(InvalidOperationException), "Do not try to get ErrorMessage if it succeeded.", () => { var errorMessage = methodCallResult.ErrorMessage; });
			Assert(!methodCallResult.ShouldBeReported);
		}

		public void TestMethodCallResultWithErrorMessage()
		{
			var methodCallResult = new MethodCallResult<int>("Error", shouldBeReported: false);
			AssertEquals(false, methodCallResult.Succeeded);
			AssertEquals("Error", methodCallResult.ErrorMessage);
			AssertExceptionThrown(typeof(InvalidOperationException), "Do not try to get Result if it failed.", () => { var result = methodCallResult.Result; });
			Assert(!methodCallResult.ShouldBeReported);
		}

		public void TestMethodCallResultWithException()
		{
			var exception = new InvalidOperationException("Exception Error");
			var methodCallResult = new MethodCallResult<int>(exception);
			AssertEquals(false, methodCallResult.Succeeded);
			AssertContains("Exception Error", methodCallResult.ErrorMessage);
			AssertExceptionThrown(typeof(InvalidOperationException), "Do not try to get Result if it failed.", () => { var result = methodCallResult.Result; });
			Assert(methodCallResult.ShouldBeReported);
		}
	}
}
