using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.HttpRetry
{
	[TestClass]
	public class HttpErrorTests
	{
		[TestClass]
		public class ParseMethod
		{
			[TestMethod]
			public void WhenGettingValidExceptionMessage_ShouldParseStatusCodeAndMessage()
			{
				// Arrange.

				const string exceptionMessage =
					"An error occurred while processing the message, refer to the details section for more information " +
					"Message ID: {8B63D40F-1ECC-4C3A-9093-4A3AB70FF1B8} " +
					"Instance ID: {D9BEC31B-06F9-46EE-B430-0D816E71901F} " +
					"Error Description: The remote server returned an error: (500) Internal Server Error.";

				// Act.

				var httpError = HttpError.Parse(exceptionMessage);

				// Assert.

				Assert.IsNotNull(httpError);
				Assert.AreEqual(500, httpError.StatusCode);
				Assert.AreEqual("Internal Server Error", httpError.Message);
			}

			[TestMethod]
			public void WhenGettingValidExceptionMessageWithoutInnerMessage_ShouldParseStatusCodeAndMessage()
			{
				// Arrange.

				const string exceptionMessage =
					"An error occurred while processing the message, refer to the details section for more information " +
					"Message ID: {8B63D40F-1ECC-4C3A-9093-4A3AB70FF1B8} " +
					"Instance ID: {D9BEC31B-06F9-46EE-B430-0D816E71901F} " +
					"Error Description: The remote server returned an error: (429).";

				// Act.

				var httpError = HttpError.Parse(exceptionMessage);

				// Assert.

				Assert.IsNotNull(httpError);
				Assert.AreEqual(429, httpError.StatusCode);
				Assert.AreEqual(string.Empty, httpError.Message);
			}

			[TestMethod]
			public void WhenGettingNonStandardExceptionMessage_ShouldPutMessageAsIs()
			{
				// Arrange.

				const string exceptionMessage = "This is non-standard HTTP exception message Unauthorized (401).";

				// Act.

				var httpError = HttpError.Parse(exceptionMessage);

				// Assert.

				Assert.IsNotNull(httpError);
				Assert.AreEqual(ushort.MaxValue, httpError.StatusCode);
				Assert.AreEqual("This is non-standard HTTP exception message Unauthorized (401).", httpError.Message);
			}
		}
	}
}
