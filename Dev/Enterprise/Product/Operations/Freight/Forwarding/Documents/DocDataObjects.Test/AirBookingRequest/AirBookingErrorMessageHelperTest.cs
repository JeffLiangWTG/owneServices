using System;
using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingErrorMessageHelperTest : TestCase
	{
		public void TestGetHumanReadableError_TaskCanceledException()
		{
			var exc = new TaskCanceledException("exception message");
			AssertEquals("eBookings service didn't respond within required time so the request could not be submitted.", AirBookingErrorMessageHelper.GetHumanReadableError(exc));
		}

		public void TestGetHumanReadableError_HttpRequestException_NoInner()
		{
			var exc = new HttpRequestException("http exception message");
			AssertEquals("http exception message", AirBookingErrorMessageHelper.GetHumanReadableError(exc));
		}

		public void TestGetHumanReadableError_HttpRequestException_Inner()
		{
			var exc = new HttpRequestException(
				"http exception message",
				new Exception("inner exception message"));

			AssertEquals("inner exception message", AirBookingErrorMessageHelper.GetHumanReadableError(exc));
		}

		public void TestGetHumanReadableError_Exception()
		{
			var exc = new Exception("exception message");
			AssertEquals("An error has occurred while processing the request.", AirBookingErrorMessageHelper.GetHumanReadableError(exc));
		}

		public void TestGetHumanReadableError_Null()
		{
			AssertEquals("An error has occurred while processing the request.", AirBookingErrorMessageHelper.GetHumanReadableError(null));
		}
	}
}
