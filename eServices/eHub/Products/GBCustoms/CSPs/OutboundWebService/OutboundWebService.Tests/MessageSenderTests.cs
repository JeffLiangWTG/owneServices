using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using System.Net;
using System.Net.Http;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Tests
{
	[TestFixture]
	public class MessageSenderTests
	{
		public MessageSender testMessageSender;
		public ILog testLogger;
		public HttpRequestMessage testRequest;

		[SetUp]
		public void Setup()
		{
			testMessageSender = new MessageSender();
			testLogger = MockRepository.GenerateMock<ILog>();
			testRequest = new HttpRequestMessage();
			testRequest.Method = HttpMethod.Post;
		}

		[Test]
		public void SendMessage_Success()
		{
			var testResult = testMessageSender.SendMessage(testLogger, "", testRequest, "https://mockendpoints-api-test.wtg.zone/SampleRest/SampleOK");

			Assert.AreEqual(HttpStatusCode.OK, testResult.Result.StatusCode);
		}
	}
}
