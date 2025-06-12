using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Controllers;
using CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers;
using Common.Logging;

using NUnit.Framework;

using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Tests
{
	[TestFixture]
	public class CSPNotificationControllerTests
	{
		[Test]
		public void TestPost()
		{
			var config = new HandlerTestBase.TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
				SubscribedMessage = GetMessage("Pentant_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("Pentant_TransportResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_TransportResponse.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			var handler = MockRepository.GeneratePartialMock<TransportAcknowledgementHandlerForTest>(Core.ProviderType.Pentant);
			handler.Expect(x => x.ExtractHeader("Authorization")).Return("HeaderAuthorization");
			handler.SetUpMock(config);
			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-Pentant"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).Repeat.Twice();
			var configuration = new HttpConfiguration();
			var request = new HttpRequestMessage();
			request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

			var pentantNotificationTestController = MockRepository.GeneratePartialMock<CSPNotificationControllerForTest>();
			pentantNotificationTestController.Request = request;
			pentantNotificationTestController.Expect(x => x.GetHandler(Arg<XDocument>.Is.Anything)).Repeat.Any().Return(handler);

			var response = pentantNotificationTestController.ReceiveNotification();

			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
		}

		[TestCase("Pentant_TransportResponse.xml")]
		[TestCase("Pentant_TransportResponse_ConvIDInBodyOnly.xml")]
		[TestCase("Pentant_TransportResponse_NoConvID.xml")]
		public void TestTransportResponse(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)));
			Assert.IsNotNull(handler);
			Assert.AreEqual(nameof(TransportAcknowledgementHandler), handler.GetType().Name);
		}

		[TestCase("Pentant_InvalidAction.xml")]
		public void TestTransportErrorResponse(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();

			var exception = Assert.Throws<HttpException>(() => controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile))));

			Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.GetHttpCode());
			Assert.AreEqual("Unknown Action 'InvalidAction'.", exception.Message);
		}

		[TestCase("Pentant_BusinessResponse.xml")]
		public void TestBusinessNotification(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)));
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
		}

		[TestCase("Pentant_NackResponse.xml")]
		public void TestNack(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)));
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(NackHandler), handler.GetType());
		}

		[TestCase("Pentant_BusinessNotification_INVRES.xml")]
		public void TestBusinessNotification_INVRES(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)));
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(INVRESHandler), handler.GetType());
		}

		[TestCase("Pentant_BusinessNotification_INVECTL.xml")]
		[TestCase("Pentant_BusinessNotification_INVEQRES.xml")]
		[TestCase("Pentant_BusinessNotification_INVRESEMRES.xml")]
		[TestCase("Pentant_BusinessNotification_INVRESEMTR.xml")]
		public void TestBusinessNotification_INVExport(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)));
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
		}

		[TestCase("Pentant_BusinessNotification_LongActionLengthWithoutRES_InvalidAction.xml")]
		public void TestBusinessNotification_LongActionLengthWithoutRES_InvalidAction(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var exception = Assert.Throws<HttpException>(() => controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile))));
			Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.GetHttpCode());
			Assert.AreEqual("Unknown Action 'urn:myvan:INVNONNNNNN'.", exception.Message);
		}

		[TestCase("Pentant_BusinessNotification_ShortActionLength_InvalidAction.xml")]
		public void TestBusinessNotification_ShortActionLength_InvalidAction(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var exception = Assert.Throws<HttpException>(() => controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile))));
			Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.GetHttpCode());
			Assert.AreEqual("Unknown Action 'urn:myvan:INVNO'.", exception.Message);
		}

		[TestCase("Pentant_BusinessNotification_INV_InvalidAction.xml")]
		public void TestBusinessNotification_INV_InvalidAction(string inputTestXmlFile)
		{
			var controller = new CSPNotificationControllerForTest();
			var exception = Assert.Throws<HttpException>(() => controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile))));
			Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.GetHttpCode());
			Assert.AreEqual("Unknown Action 'urn:myvan:INVNON'.", exception.Message);
		}

		string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Tests.TestFiles.{source}").ReadToEnd();
		}
	}

	#region Implementation
	public class CSPNotificationControllerForTest : PentantNotificationController
	{
		protected internal override Stream InternalRequestStream => Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Tests.TestFiles.Pentant_TransportResponse.xml");

		public CSPNotificationControllerForTest()
		{
			InternalLog = () => MockRepository.GenerateMock<ILog>();
		}

		public IMessageHandler ExposeHandler(XDocument responseXml)
		{
			return GetHandler(responseXml);
		}
	}
	#endregion
}
