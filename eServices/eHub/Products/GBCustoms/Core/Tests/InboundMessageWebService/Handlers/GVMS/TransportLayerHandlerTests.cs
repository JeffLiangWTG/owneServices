using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.GVMS
{
	[TestFixture]
	public class TransportLayerHandlerTests : HandlerTestBase
	{
		const string RegistrationType = "GBCustoms-Transport";

		[Test]
		[TestCase("GVMS_TransportLayerResponse_XMLMessage.json")]
		[TestCase("GVMS_TransportLayerResponse_JSONMessage.json")]
		public void TestTransportLayerHandlerHandleBusinessResponseForXmlMessage(string inputFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				SubscriptionValueGmrId = inputFile == "GVMS_TransportLayerResponse_JSONMessage.json" ? "GMRI000002FK" : null,
				GmrIdSubscriptionExists = false,
				ClientRegistrationType = RegistrationType,
				ExpectedRecipients = new[] { "recipient" },
				ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true,
				ClientPermittedRecipient = true,
			};
			handler.SetUpMock(config,
				inputFile.Equals("GVMS_TransportLayerResponse_JSONMessage.json") ? MessageHeaderConfigurationForJSON : MessageHeaderConfiguration);

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustomsTest-GVMS"), Arg<string>.Is.Equal("GBCustomsTest"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual($"recipient", message.ClientID);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				Assert.AreEqual(GetMessage(inputFile.Equals("GVMS_TransportLayerResponse_JSONMessage.json") ? "InboxAndOutbox - JSON.xml" : "InboxAndOutbox - XML.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
			});

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		[TestCase("GVMS_TransportLayerResponse_XMLMessage.json")]
		public void TestTransportLayerHandlerValidateIdAndGetRecipients_WhenNoMessageIdIsFound_ShouldThrowHttpException(string inputFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				ClientRegistrationType = RegistrationType,
				ExpectedRecipients = null,
				ExpectedClientRegistrationForCorrelation = CreateInvalidClientRegistrations(),
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForLogger = true,
				IncludeMessageHeaders = true
			};

			handler.SetUpMock(config, MessageHeaderConfiguration);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: GVMS. No Message ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		[TestCase("GVMS_TransportLayerResponse_XMLMessage.json")]
		public void TestTransportLayerHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException(string inputFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				ClientRegistrationType = RegistrationType,
				ExpectedRecipients = null,
				ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true
			};

			handler.SetUpMock(config, MessageHeaderConfiguration);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(465, exception.GetHttpCode());
			Assert.AreEqual("No subscriber found for Message ID: [90b6abd2-f7f9-4223-97c0-29f0c10bbf8e]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestTransportLayerHandlerGetRecipients_FallBackToGmrId(bool gmrSubscriptionExists)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustomsTest-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				SubscriptionValueGmrId = "GMRI000002FK",
				ClientRegistrationType = RegistrationType,
				ExpectedRecipients = null,
				GmrIdSubscriptionExists = gmrSubscriptionExists,
				ExpectedRecipientsGmrId = new[] { "GMR-Recipient" },
				ExpectedClientRegistrationForCorrelation = CreateClientRegistrations(),
				RequestContent = GetMessage("GVMS_TransportLayerResponse_JSONMessage.json"),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForClientRegistrationAccessorForCorrelation = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true,
				UseMockForClientAccessor = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config, MessageHeaderConfigurationForJSON);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);

			if (gmrSubscriptionExists)
			{
				config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustomsTest-GVMS"), Arg<string>.Is.Equal("GBCustomsTest"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
					Assert.AreEqual("GMR-Recipient", message.ClientID);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
					Assert.AreEqual(GetMessage("InboxAndOutbox - JSON.xml"), message.MessageStream.DecodeAndDecompress().ReadToEnd());
				});

				var response = handler.ExecuteProcessTest();
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				config.VerifyAll();
			}
			else
			{
				var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
				Assert.AreEqual(465, exception.GetHttpCode());
				Assert.AreEqual("No subscriber found for Message ID: [90b6abd2-f7f9-4223-97c0-29f0c10bbf8e]", exception.Message);
				Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
				Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);
			}

			handler.VerifyAllExpectations();
		}

		[Test]
		[TestCase("GVMS_TransportLayerResponse_XMLMessage.json")]
		public void TestTransportLayerHandlerAuthorisation_ShouldThrowHttpException(string inputFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				ExpectedRecipients = null,
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = true
			};

			handler.SetUpMock(config, IncorrectSignatureMessageHeaderConfiguration);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(464, exception.GetHttpCode());
			Assert.IsTrue(exception.Message.StartsWith("Could not verify the push request for the test, uat or production secret."));
		}

		[Test]
		[TestCase("GVMS_TransportLayerResponse_JSONMessage.json")]
		public void TestTransportLayerHandlerValidateIdAndGetRecipients_WhenNoHeadersAreIncluded(string inputFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-GVMS",
				SubscriptionType = "GBCTID",
				SubscriptionValue = "90b6abd2-f7f9-4223-97c0-29f0c10bbf8e",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage(inputFile),
				MessageDoc = null,
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = null,
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = false,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				IncludeMessageHeaders = false,
				Logger = new MockLogger()
			};

			handler.SetUpMock(config, MessageHeaderConfiguration);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTestWithNoHeaders());

			Assert.AreEqual(463, exception.GetHttpCode());
			Assert.AreEqual("No Signature header (x-hub-signature) received.", exception.Message);
			var logString = ((MockLogger)config.Logger).Log;
			Assert.IsTrue(logString.Contains("Trace - [Request Message: {") && logString.Contains("86bb5f82-452e-49c8-88af-1f321d573960"));
			Assert.IsTrue(logString.Contains("Trace - [Request Headers: ]"));
		}

		[Test]
		public void TestValidateSignature_HMRCExample_Success()
		{
			var signature = "c6cdd3e30021fe66d88d37088fed2566453eb7fb";
			var pushSecret = "sample key";
			var messageString = "{\"sample\": \"payload\"}";
			Assert.IsTrue(handler.ValidateSignatureHeader(signature, pushSecret, messageString), "The signature that is calculated should be the same as the provided signature, but does not.");
		}

		[Test]
		public void TestValidateSignature_Failure()
		{
			var signature = "c6cdd3e30021fe66d88d37088fed2566453eb7fb";
			var pushSecret = "sample key";
			var messageString = "{\"sample\": \"payload1\"}";
			var calculatedSignature = handler.ValidateSignatureHeader(signature, pushSecret, messageString);
			Assert.IsFalse(handler.ValidateSignatureHeader(signature, pushSecret, messageString), "The signature that is calculated should differ from the provided signature, but does not.");
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<TransportLayerHandlerForTest>(ProviderType.GVMS);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}
		}

		private TransportLayerHandlerForTest handler;

		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"GBCustomsID", "GBCustoms"},
			{"GBCustomsTestID", "GBCustomsTest"},
			{"GBCustomsRegistrationType", "GBCustoms-Direct"},
			{"GBCustomsSubscriptionType", "GBCTID"},
			{"PushSecretTest", "L5HRYEDCF7GWLHUWPIF5HYIZ6GPSEBMF" },
			{"PushSecretUat", "YJ2CRF2USGNW3OC2E3B7FB265XG33MBP" },
			{"PushSecretProd", "TBC" },
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};

		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"x-hub-signature", "1AD38A3B87C16760BBAED3AA3634198DADA95E65"}
		};

		Dictionary<string, string> MessageHeaderConfigurationForJSON = new Dictionary<string, string>
		{
			{"x-hub-signature", "03924C67B046C295547E3805A4C147C944B70659"}
		};

		Dictionary<string, string> IncorrectSignatureMessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"x-hub-signature", "INCORRECT_SIGNATURE"}
		};

		List<Dictionary<string, object>> CreateClientRegistrations()
		{
			return new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Create-2" },
					{ "CX_Code", "Header:Notification-Box-Id" },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Body-1" },
					{ "CX_Code", "JSONBody:boxId" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Body-2" },
					{ "CX_Code", "JSONBody: messageContentType" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "ContentType" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Headers-1" },
					{ "CX_Code", "Header:Notification-Message-Id" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationMessageId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Headers-2" },
					{ "CX_Code", "Header:Notification-Box-Id" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Json" },
					{ "CX_Code", "JSONBody:message.messageId" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Subscribed" },
					{ "CX_Attr1", "MessageId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Xml" },
					{ "CX_Code", "XMLBody://*[local-name() = 'messageId']/text()" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Subscribed" },
					{ "CX_Attr1", "MessageId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Update-2" },
					{ "CX_Code", "Header:Notification-Box-Id" },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
			};
		}

		List<Dictionary<string, object>> CreateInvalidClientRegistrations()
		{
			return new List<Dictionary<string, object>>()
			{
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Create-2" },
					{ "CX_Code", "Header:Notification-Box-Id" },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Update-2" },
					{ "CX_Code", "Header:Notification-Box-Id" },
					{ "CX_Flag1", "Synchronous"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "NotificationBoxId" }
				},
				new Dictionary<string, object>
				{
					{ "CX_CC_ID", "GBCustomsTest-GVMS" },
					{ "CX_Qualifier", "Notification-Body-2" },
					{ "CX_Code", "JSONBody: messageContentType" },
					{ "CX_Flag1", "Notification"},
					{ "CX_Flag2", "Additional" },
					{ "CX_Attr1", "ContentType" }
				},
			};
		}

		public override string Provider => "GVMS";
	}
}
