using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using CargoWise.eHub.Products.GBCustoms.TL.InboundMessageWebService.Controllers;
using Common.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using static CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.HandlerTestBase;

namespace CargoWise.eHub.Products.GBCustoms.TL.InboundMessageWebService.Tests
{
    public class InboundMessageControllerTest
    {
		[Test]
		public void TestGetHandler()
		{
			var controller = new InboundMessageControllerForTest();
			var handler = controller.ExposeHandler(ProviderType.CTCGB);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportLayerHandler).Name, handler.GetType().Name);
		}

		[Test]
		public void TestValidateProviderType()
		{
			var controller = new InboundMessageControllerForTest();
			try
			{
				controller.ExposeValidateProvider(ProviderType.CTCGB);
				controller.ExposeValidateProvider(ProviderType.GVMS);
				controller.ExposeValidateProvider(ProviderType.CDS);
				Assert.Fail("The method should fail, because of an invalid provider type - CDS");
			}
			catch (Exception ex)
            {
				Assert.AreEqual(ex.Message, "The Provider Type [CDS] is not valid for this web service.");
			}
		}

		[Test]
		public void TestGetReceiveJsonString()
		{
			var controller = new InboundMessageControllerForTest();
			var result = controller.ExposeGetReceiveJsonString("transport_layer_challenge_test");
			Assert.AreEqual(result.ToString(), "{\r\n  \"challenge\": \"transport_layer_challenge_test\"\r\n}");
		}

		[Test]
		public void TestControllerHTTPException()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();

			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Open,
				RequestContent = "",
				UseMockForLogger = true,
				Logger = mockLogger
			};

			var controller = new InboundMessageControllerForTest(config);
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			controller.EhubTransactionsContext = () => mockContext;
			var response = controller.ReceivePost("InvalidProvider");
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

			mockLogger.VerifyAllExpectations();
		}

		[Test]
		public void TestControllerException()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockMessageContentLogger = MockRepository.GenerateMock<ILog>();

			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Open,
				RequestContent = "",
				UseMockForLogger = true,
				Logger = mockLogger,
				MessageContentLogger = mockMessageContentLogger
			};

			var controller = new InboundMessageControllerForTest(config) { shouldTrowException = true };
			Assert.Throws<Exception>(() => controller.ReceivePost("CTCGB"));

			mockLogger.VerifyAllExpectations();
		}

		private TestDbSet<eHubClient> CreateGBCustomsClient()
		{
			var ehubClients = new TestDbSet<eHubClient>();
			ehubClients.Add(new eHubClient()
			{
				CC_PK = new Guid("00000000-0000-0000-0000-000000000000"),
				CC_ID = "GBCustoms"
			});

			return ehubClients;
		}

	}

	#region Implementation
	public class InboundMessageControllerForTest : InboundMessageController
	{
		private TestConfiguration config;
		public bool shouldTrowException = false;

		public InboundMessageControllerForTest()
		{
			InternalLog = () => MockRepository.GenerateMock<ILog>();
		}

		public InboundMessageControllerForTest(TestConfiguration config)
        {
			this.config = config;
			logger = this.config.Logger;
			messageContentLogger = this.config.MessageContentLogger;
			EhubTransactionsContext = () => new eHubTransactionsContext();
		}

		public IMessageHandler ExposeHandler(ProviderType providerType)
		{
			return GetHandler(providerType);
		}

		public void ExposeValidateProvider(ProviderType providerType)
        {	
			ValidateProvider(providerType);
        }

		public JObject ExposeGetReceiveJsonString(string challenge)
		{
			return GetReceiveJsonString(challenge);
		}

		public override HttpRequestMessage GetRequest()
		{
			if(shouldTrowException)
            {
				throw new Exception("TestException");
            }

			var request = new HttpRequestMessage(HttpMethod.Post, "");
			request.SetConfiguration(new System.Web.Http.HttpConfiguration());
			request.Content = new StringContent(config.RequestContent);
			return request;
		}
	}
	#endregion
}
