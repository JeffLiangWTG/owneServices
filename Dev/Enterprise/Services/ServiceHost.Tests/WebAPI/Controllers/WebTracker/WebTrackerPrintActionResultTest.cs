using System.IO;
using System.Net;
using System.Threading;
using Enterprise.Tracking.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class WebTrackerPrintActionResultTest : TransactionedTestCase
	{
		public void TestExecute_NoResult()
		{
			var result = new WebTrackerPrintActionResult(null);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
		}

		public void TestExecute_ErrorMessage()
		{
			var mockPrintResult = new Mock<IWebTrackerPrintResult>();
			mockPrintResult.SetupGet(x => x.ErrorMessage).Returns("Some error message");

			var result = new WebTrackerPrintActionResult(mockPrintResult.Object);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertEquals("Some error message", response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			});
		}

		public void TestExecute_File()
		{
			var fileContents = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
			var mockPrintResult = new Mock<IWebTrackerPrintResult>();
			mockPrintResult.SetupGet(x => x.FileName).Returns("FileName.pdf");
			mockPrintResult.SetupGet(x => x.FileContents).Returns(new MemoryStream(fileContents));

			var result = new WebTrackerPrintActionResult(mockPrintResult.Object);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			CombineAssertions(() =>
			{
				AssertEquals(fileContents, response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
				AssertEquals("application/pdf", response.Content.Headers.ContentType.ToString());
				AssertEquals("inline; filename=FileName.pdf", response.Content.Headers.ContentDisposition.ToString());
			});
		}
	}
}
