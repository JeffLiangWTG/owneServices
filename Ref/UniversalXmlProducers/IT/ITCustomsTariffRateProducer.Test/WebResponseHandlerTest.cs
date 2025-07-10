using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class WebResponseHandlerTest
	{
		[Test]
		public void GetWebRepsonseForTariffAsync_RetryPolicyUpToFourAttempts()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					requestCount++;
					return GetNewUnsuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebRepsonseForTariffAsync(DummyUrl, "1111111111").Result;
			Assert.AreEqual(4, requestCount, "Total Request Count");
			Assert.AreEqual("Unable to get response from website", lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForTariffAsync_RetryPolicyStopAtSuccessfullResponse()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					if (requestCount++ < 1)
						return GetNewUnsuccesfullResponse();
					return GetNewSuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebRepsonseForTariffAsync(DummyUrl, "1111111111").Result;
			Assert.AreEqual(2, requestCount, "Total Request Count");
			Assert.AreEqual(DummyMessage, lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForCertificateLinkAsync_RetryPolicyUpToFourAttempts()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					requestCount++;
					return GetNewUnsuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebRepsonseForCertificateLinkAsync(DummyUrl, string.Empty).Result;
			Assert.AreEqual(4, requestCount, "Total Request Count");
			Assert.AreEqual("Unable to get response from website", lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForCertificateLinkAsync_RetryPolicyStopAtSuccessfullResponse()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					if (requestCount++ < 1)
						return GetNewUnsuccesfullResponse();
					return GetNewSuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebRepsonseForCertificateLinkAsync(DummyUrl, string.Empty).Result;
			Assert.AreEqual(2, requestCount, "Total Request Count");
			Assert.AreEqual(DummyMessage, lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForCertificateAsync_RetryPolicyUpToFourAttempts()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					requestCount++;
					return GetNewUnsuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebResponseForCertificateAsync(DummyUrl, "30,1,-2,6,'CSA','1000','95030000','00','T','031','25/02/2013'", "javascript:linkToPostKeySicuroBill2('MisureServlet',5, 1, -2, 100, 'C', '085', '14/12/2019' )", "1111111111").Result;
			Assert.AreEqual(4, requestCount, "Total Request Count");
			Assert.AreEqual("Unable to get response from website", lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForCertificateAsync_RetryPolicyStopAtSuccessfullResponse()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.PostAsync(DummyUrl, It.IsAny<HttpContent>()))
				.Returns(() =>
				{
					if (requestCount++ < 1)
						return GetNewUnsuccesfullResponse();
					return GetNewSuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebResponseForCertificateAsync(DummyUrl, "30,1,-2,6,'CSA','1000','95030000','00','T','031','25/02/2013'", "<a href=\"javascript:linkToPostKeyBill('MisureServlet',30,1,-2,6,'CSA','1000','95030000','00','T','031','25/02/2013')\">Certificato</a>", "1111111111").Result;
			Assert.AreEqual(2, requestCount, "Total Request Count");
			Assert.AreEqual(DummyMessage, lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForPublicationDateAsync_RetryPolicyUpToFourAttempts()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.GetAsync(DummyUrl))
				.Returns(() =>
				{
					requestCount++;
					return GetNewUnsuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebResponseForPublicationDateAsync(DummyUrl).Result;
			Assert.AreEqual(4, requestCount, "Total Request Count");
			Assert.AreEqual("Unable to get response from website", lastResponse.ResponseMessage, "Response Message");
		}

		[Test]
		public void GetWebRepsonseForPublicationDateAsync_RetryPolicyStopAtSuccessfullResponse()
		{
			int requestCount = 0;
			httpClientMock
				.Setup(x => x.GetAsync(DummyUrl))
				.Returns(() =>
				{
					if (requestCount++ < 1)
						return GetNewUnsuccesfullResponse();
					return GetNewSuccesfullResponse();
				});

			var responseHandler = new WebResponseHandler(httpClientMock.Object, dateTimeProvider);
			var lastResponse = responseHandler.GetWebResponseForPublicationDateAsync(DummyUrl).Result;
			Assert.AreEqual(2, requestCount, "Total Request Count");
			Assert.AreEqual(DummyMessage, lastResponse.ResponseMessage, "Response Message");
		}

		[SetUp]
		public void Setup()
		{
			httpClientMock = new Mock<IHttpHandler>();
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01));
			dateTimeProvider = dateTimeProviderMock.Object;
		}

		Mock<IHttpHandler> httpClientMock;
		IDateTimeProvider dateTimeProvider;

		const string DummyUrl = "http://localhost/dummyurl";
		const string DummyMessage = "<html><body>some dummy message</body></html>";

		static Task<HttpResponseMessage> GetNewUnsuccesfullResponse()
		{
			return Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent(DummyMessage) });
		}

		static Task<HttpResponseMessage> GetNewSuccesfullResponse()
		{
			return Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(DummyMessage) });
		}
	}
}
