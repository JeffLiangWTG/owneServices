using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class RequestHandlingExceptionTest : TestCase
	{
		public void TestMessage_ShouldBeInnerExceptionMessage()
		{
			var innerExceptionMessage = "Some web problem";

			var ex = new RequestHandlingException(null, null, new WebException(innerExceptionMessage));

			AssertEquals(innerExceptionMessage, ex.Message);
		}

		public void TestData_ShouldContainsSeachParameters()
		{
			var searchParameters = "LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU";

			var ex = new RequestHandlingException(searchParameters, null, null);

			AssertEquals(searchParameters, ex.Data["Search Parameters"]);
		}

		public void TestData_ShouldContainsResponseInfo()
		{
			var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
			{
				ReasonPhrase = "Bad request",
				Content = new StringContent("Bad request Content"),
			};
			message.Headers.Add("Connection", "close");
			message.Headers.Add("Cache-Control", "no-cache");
			message.RequestMessage = new HttpRequestMessage(new HttpMethod("Get"), "uri");
			message.StatusCode = HttpStatusCode.BadRequest;

			var ex = new RequestHandlingException(null, message, null);

			AssertEquals(message.Headers.ToString(), ex.Data["Response Header"]);
			AssertEquals(message.RequestMessage.ToString(), ex.Data["Response Request"]);
			AssertEquals(message.StatusCode, ex.Data["Response Status"]);
		}

		public void TestData_ShouldContainsOSVersion_WhenExceptionIsHttpRequestException()
		{
			var innerExceptionMessage = "Some Http problem";
			var ex = new RequestHandlingException(null, null, new HttpRequestException(innerExceptionMessage));

			AssertEquals(System.Environment.OSVersion.VersionString, ex.Data["Client OS Version"]);
		}

		public void TestData_ShouldContainsHttpRequestExceptionData_WhenExceptionIsHttpRequestException()
		{
			var httpRequestDataKey = "Content";
			var httpRequestDataValue = "Value";
			var httpRequestException = new HttpRequestException(string.Empty);
			httpRequestException.Data.Add(httpRequestDataKey, httpRequestDataValue);
			var ex = new RequestHandlingException(null, null, httpRequestException);

			AssertEquals(httpRequestDataValue, ex.Data[httpRequestDataKey]);
		}

		public void TestData_ShouldContainsResponseHeader_WhenInnerExceptionIsWebException()
		{
			string message = "The remote server returned an error: (404) Not Found.";
			var response = CreateResponse(message);

			var webException = new WebException(message, null, WebExceptionStatus.ProtocolError, response);
			var exception = new HttpRequestException(string.Empty, webException);
			var ex = new RequestHandlingException(null, null, exception);
			var expectedResult = new ZString(webException.Response.Headers.ToString()).SubstringSafe(0, 1000);
			AssertEquals(expectedResult, ex.Data["Web Response Header"]);
		}

		public void TestData_ShouldContainsResponseContent_WhenInnerExceptionIsWebException()
		{
			string message = "The remote server returned an error: (404) Not Found.";
			var response = CreateResponse(message);

			var webException = new WebException(message, null, WebExceptionStatus.ProtocolError, response);
			var exception = new HttpRequestException(string.Empty, webException);
			var ex = new RequestHandlingException(null, null, exception);
			AssertNotNull(ex.Data["Response Content"]);
		}

		WebResponse CreateResponse(string message)
		{
			var httpResponseMock = new Mock<HttpWebResponse>();
			httpResponseMock.Setup(response => response.StatusCode).Returns(HttpStatusCode.NotFound);
			httpResponseMock.Setup(response => response.GetResponseStream()).Returns(new MemoryStream(new UTF8Encoding().GetBytes(message)));
			httpResponseMock.Setup(response => response.Headers).Returns(new WebHeaderCollection());
			return httpResponseMock.Object;
		}
	}
}
