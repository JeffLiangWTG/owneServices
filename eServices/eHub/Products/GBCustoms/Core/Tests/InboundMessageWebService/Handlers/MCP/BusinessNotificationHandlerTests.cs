using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.MCP
{
	[TestFixture]
	public class BusinessNotificationHandlerTests : HandlerTestBase
	{
		[TestCase("MCP_BusinessResponse_NoConvoId.xml")]
		[TestCase("MCP_DMS_NotificationType_BusinessResponse_NoConvoId.xml")]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenConversationIdIsMissing_ShouldThrowHttpException(string inputXmlFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = null,
				RequestContent = GetMessage(inputXmlFile),
				MessageDoc = XDocument.Parse(GetMessage(inputXmlFile)),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: MCP. No Conversation ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = null,
				RequestContent = GetMessage("MCP_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				UseMockForClientAccessor = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(465, exception.GetHttpCode());
			Assert.AreEqual("No subscriber found for Conversation ID: [40ad9ef5-2d57-4286-a12a-31675ffe4d9b]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		[TestCase("MCP_BusinessResponse.xml")]
		[TestCase("MCP_DMS_NotificationType_BusinessResponse.xml")]
		public void TestBusinessHandlerHandleBusinessResponse(string inputXmlFile)
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				ClientRegistrationType = "GBCustoms-MCP",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage(inputXmlFile),
				MessageDoc = XDocument.Parse(GetMessage(inputXmlFile)),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			var insertCount = 1;
			handler.SetUpMock(config);
			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-MCP"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				namespaceManager.AddNamespace("urn", "urn:wco:datamodel:WCO:DocumentMetaData-DMS:2");
				Assert.AreEqual("40ad9ef5-2d57-4286-a12a-31675ffe4d9b", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:ConversationID", namespaceManager).Value);
				Assert.AreEqual("eHubTrackingID", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody/urn:MetaData", namespaceManager));
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponseInvalidRecipient()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				ClientRegistrationType = "GBCustoms-MCP",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("MCP_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_BusinessResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = false,
			};

			handler.SetUpMock(config);

			var expectedError = new eHubError()
			{
				EE_Alerted = false,
				EE_Description = "GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.",
				EE_ErrorDetail = "<ErrorDetail>GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.</ErrorDetail>",
				EE_ErrorType = "EXP",
				EE_Source = "HUB"
			};

			var expectedMessage = new eHubInboxMessage()
			{
				EI_CC_Recipient = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_ApplicationCode = "HUB",
				EI_MessageType = "CDS",
				EI_CC_Sender = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_Status = 255,
				EI_Content = "H4sIAAAAAAAEAK1WWZOaShR+TqryHyhfc82wiJGpmaRUBDGCiizCG0uPLM1yR0Dh198DOs6SuTUPSfki3Wf5zne+7j6zk4fyIsxSgkeFE+LD7ZfPMjocnD0i/AwdiDQrCCfPkfNIFBkROBUissdwH6ZOgXzi4TFLCDM8oAJ5ASHizHUw7BMeDlFaEOHZP0ePSVi09hDiEXkohCjJOcvhH8LL0kOZhOmeKILr+rcvn9uflLpZmfrEE6Y5cnz02KL8fW+S+TVs3P08JZiowArKuu9R38gegVIv8yHDfU/XhP6oRxwKJ/UdnKXovlejQ+/nD3AErOFD6DktHwfAmofefW86Nnc9wFimBQTrgd2nV4ZE6N/3hux559PdvyUqkc8DO1qYoB80SY36JNenhxo1uKXZW4b+xnLkV5K6Jcm7mzfWXQQXyvixnhu1G+WKvVMDVRRIazsZeE3eWHSAXVOQXVoJfBFXbkSG/m5RrlI/cps8tsDeNbnYNo9Dg1TWq1itLMYobJMlNdMgLV0Fv1mhzihtFUn7ZTioVJGLfJPCbqo+5zNG9Y5UFb1mBx69CaV5ULgi26zSFtcf5XwRK2gcjQyduUp6fFYtGZ/xa5aRa7byEq+So/FR3nKWpp+0c92zwjHZxheF0qL1V3HW20Xmz9XjKhxVEIVZpl6zTLjarke1zI9PSypQNEGJHNEorK30dbqPR6656fB29epUZYvGwUiM2qOB11iBb32PROrgpvIQbDmpqz1nLHo0tFsf8+yzwj6vNfkMekK5plH6MwoDxlnLy1IHDpuslqJBs4ys0ZKm4s6ebDkRFJdWsSvY2EuV3KVZvv1e77PFWrzYUSrricYKcuFrvQkVQ72UlwyGPq1Uq6SLVXSx+Hy3IUfDd3u6pWaaIQ/lUPqqzgx+PeUKu7PndLSbYC0RClsb/Jpqp3Z9a++UI2BqHHNxsHWl8qGfXoo7jH8TixYfL7wssMdMKhe4sJIT3tAcBT410rteXHumJ0bjiVwJfQpd0ZjYtFFaDP77HM2NwE3097GZPnYThX3D2ROWiccouU2z2J4plMeolWviIeih7bFp7xbAKVdCfbEN59o1TyWcbfD/s3O1Tq16Gc3OeGMhBbsI6UIDPKVuYsQbxoCzwxXOLj/fHfgd3c3HL/gF/c+VfJVezo1+qixaIFvsH3Grx4Z21f50UC6pVsvcVjWUBxlzLcclxI6lqXQ9Y95H9eOFoNfP/QE9dHjVtOVcfa4jGp9A28dVbFPQI/LKddunTTzy+FwELUcdB4lwgHoT6EnbT7zVN18VWmIsU2ftSGcUXgkVXoI7JEhWvMXIkVzLtBoppkzZ/Hi0ZF7lD1xhgW24Q1rubR3PWk5BG4BFxRZ9Crzkwl+7fsYysWgFe3OoFfK3GrQNiGV2er/YSMPndUPz54vcTby9DfcbvAXkuTYylJsxKYVXPR3lj+7pZ01juOs1wPh0V0G9M0Zu627GJ/h/lLVj9T84zji7/Q9q6Xh47ollnra2CRpMgD/aWKg8a5lCYJsGdkycQU6pUXidVpo9+7afv/t2ve300PZQ1oL5JsbOyrB/KaQgyuTgqFPWJc6lL2f+r+fx7NfV8Wqt01nn93sPu/UXPMIaaOLN2w26u7vpnvXugQ/OM0z38fRFpE4Co8g0S9vBpRssJL5HVA4uYXlAOj6HHtg+7bPf+wN4f/oORTt9hhp+Zx8e0MDn3N7NewF3/Ynj71H/RTAYaN63heQFTG19rc7R1RpmP3yZdG5grnrf8yRP15L/MkOfYpnv3JBhWWrEcBevu5sXpd/dvJyi2vHr1UJr8+Xzf5aol0afCgAA"
			};

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			handler.ContextFactory = () => mockContext;

			var response = handler.ExecuteProcessTest();

			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			Assert.AreEqual(1, mockContext.eHubErrors.Count());
			Assert.AreEqual(1, mockContext.eHubInboxMessages.Count());

			var error = mockContext.eHubErrors.First();
			var inboxMessage = mockContext.eHubInboxMessages.First();

			Assert.AreEqual(expectedError.EE_Alerted, error.EE_Alerted);
			Assert.AreEqual(expectedError.EE_Description, error.EE_Description);
			Assert.AreEqual(expectedError.EE_ErrorDetail, error.EE_ErrorDetail);
			Assert.AreEqual(expectedError.EE_ErrorType, error.EE_ErrorType);
			Assert.AreEqual(expectedError.EE_Source, error.EE_Source);

			Assert.AreEqual(expectedMessage.EI_Status, inboxMessage.EI_Status);
			Assert.AreEqual(expectedMessage.EI_CC_Sender, inboxMessage.EI_CC_Sender);
			Assert.AreEqual(expectedMessage.EI_CC_Recipient, inboxMessage.EI_CC_Recipient);
			Assert.AreEqual(expectedMessage.EI_ApplicationCode, inboxMessage.EI_ApplicationCode);
			Assert.AreEqual(expectedMessage.EI_MessageType, inboxMessage.EI_MessageType);
			Assert.AreEqual(expectedMessage.EI_Content, inboxMessage.EI_Content);

			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponse_NoConvID_ButValidFallback()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "Sani_ST2_7177105",
				ClientRegistrationType = "GBCustoms-MCP",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("MCP_BusinessResponse_InvalidConv_ValidFuncRef.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_BusinessResponse_InvalidConv_ValidFuncRef.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			var insertCount = 1;
			handler.SetUpMock(config);
			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-MCP"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				namespaceManager.AddNamespace("urn", "urn:wco:datamodel:WCO:DocumentMetaData-DMS:2");
				Assert.AreEqual("InvalidConversationID", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:ConversationID", namespaceManager).Value);
				Assert.AreEqual("eHubTrackingID", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody/urn:MetaData", namespaceManager));
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponse_NoConvID_NonCw1Fallback_ShouldConsume()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "GB775395776000-0000031",
				ClientExist = true,
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "GBCustoms" },
				RequestContent = GetMessage("MCP_BusinessResponse_InvalidFuncRef.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_BusinessResponse_InvalidFuncRef.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);

			var expectedError = new eHubError()
			{
				EE_Alerted = false,
				EE_Description = "GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.",
				EE_ErrorDetail = "<ErrorDetail>GBCustoms Inbound Message - Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message.</ErrorDetail>",
				EE_ErrorType = "EXP",
				EE_Source = "HUB"
			};

			var expectedMessage = new eHubInboxMessage()
			{
				EI_CC_Recipient = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_ApplicationCode = "HUB",
				EI_MessageType = "CDS",
				EI_CC_Sender = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_Status = 255,
				EI_Content = "H4sIAAAAAAAEAK1WW3OiShB+36r8B8rXnGy4iEdSyW6pCOIKKnIR3rjMymW4nAgo/PrToGtMsqk87JZlKT3dPV9/X8/Q06OH8iLMUoJHhRPi/cPNFxnt984OEX6G9kSaFYST58h5JoqMCJwKEdlzuAtTp0A+8fM5Swgz3KMCeQEh4sx1MKwTHg5RWhDhKT5Hz0lYtP6Q4hl5KIQsyWmX/T+El6X7MgnTHVEEF/vXmy/tR0rdrEx94hemGXJ89NyifL82zvwaFh6/HxNMVOAFZT31qK9kj0Cpl/mww1NP14S7YY/YF07qOzhL0VOvRvve928QCFjDn6HntHzsAWseek+9ycjc9gBjmRaQrAd+BPHKkwj9p96APS/B4n8lKpHPA0FamKBvNEkN70jujh5oVP+BZh8Y+ivLkbck9UCSj/dvvM85XKjl22pm1G6UK/ZWDVRRIK3NuO81eWPRAXZNQXZpJfBFXLkRGfrbeblM/cht8tgCf9fkYts8DAxSWS1jtbIYo7BNltRMg7R0FeKmhTqltGUk7RZhv1JFLvJNCrup+rKfMay3pKroNdv36HUozYLCFdlmmba4/mjPq1xB42hk6MxU0uOzasH4jF+zjFyzlZd4lRyNDvKGszT9qJ3qnhaOyTa+KJQWrb/Ks9rMM3+mHpbhsIIszCL1mkXC1XY9rGV+dFxQgaIJSuSIRmFtpNvJLh665rrD29WrU5UtGnsjMWqPBl5jBZ71HRKpvZvKA/DlpK72nLHo4cBuY8xTzBL7vNbkU9CEck2j9KcUBozTlpeFDhw2WS1F/WYRWcMFTcWdP9lyIigurWJXsLGXKrlLs3z7vNpl85V49qNU1hONJeyFL/UmVAz1Ul7SH/i0Ui2TLlfR5eLz7ZocDn6r6YaaaoY8kEPpVp0a/GrCFXbnz+loO8ZaIhS21v8x0Y6tfWNvlQNgahxzvrd1pfJBTy/FHca/iUWLD2de5thjxpULXFjJEa9pjoKYGumdFhfN9MRoPJErQafQFY2xTRulxeC/z9HMCNxE/z0208duorBvOPuFZewxSm7TLLanCuUxauWaeAD90Gps2ts5cMqVUF9sw7l2zWMJZxvi/+xcrVKrXkTTE95YSMEvQrrQAE+pmxjxmjHg7HCFs81Pdwf+Td/NRlf8Qv/PlHyZns+NfqwsWiBb7J9xq8eGdun9Sb9cUG0vcxvVUH7KmGs5LiF3LE2kyxnzPqsfzwW9ftEH+qHDq6Yt5+pLHdHoCL19WMY2BRqRF65bndbx0ONzEXo56jhIhD3Um4AmrZ54o69vFVpiLFNn7UhnFF4JFV6COyRIlrzFyJFcy7QaKaZM2fxouGBe7R+4whzbcIe03Ns6nracQm8AFhVb9DHwkjN/rf2EZWzRCvZmUCvs3/agbUAus+v3s480eLEbmj+b527i7Wy43+BdQJ5qI0O5GZFSeOmng/zZPf3S0xjueg0w/rqroN4pI7d1N6Mj/D/I2qH6AMcJZ7f+SS0dDy+aWOZxY5vQgwnwRxtzlWdn6whOjiaziga/7f0/IQ9wf8NXfqfp+/hO364nWh1lLZitY+wsDfuHQgqiTPYPOmWxpzxnbU4aXM7kKa6r5ZWt67Uu7r2Onf2KS7BBX7x5f0PvPd53r/bzaz44jTPnx4uBSJ0EBpNJlrZjTDdlSHyPqBxcgllK4U/ov1m9/yDJ9m7s+Dt0d5UARpoP3SFrAaPbnVbn6BIAAyA+Tzv3MFx9GHyUJyvJv97njmKZf7kBw7LUkOEugY/315U/3l/PU+0k9srQOt18+R8rD+qqqgoAAA=="
			};

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());
			handler.ContextFactory = () => mockContext;

			var response = handler.ExecuteProcessTest();

			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			Assert.AreEqual(1, mockContext.eHubErrors.Count());
			Assert.AreEqual(1, mockContext.eHubInboxMessages.Count());

			var error = mockContext.eHubErrors.First();
			var inboxMessage = mockContext.eHubInboxMessages.First();

			Assert.AreEqual(expectedError.EE_Alerted, error.EE_Alerted);
			Assert.AreEqual(expectedError.EE_Description, error.EE_Description);
			Assert.AreEqual(expectedError.EE_ErrorDetail, error.EE_ErrorDetail);
			Assert.AreEqual(expectedError.EE_ErrorType, error.EE_ErrorType);
			Assert.AreEqual(expectedError.EE_Source, error.EE_Source);

			Assert.AreEqual(expectedMessage.EI_Status, inboxMessage.EI_Status);
			Assert.AreEqual(expectedMessage.EI_CC_Sender, inboxMessage.EI_CC_Sender);
			Assert.AreEqual(expectedMessage.EI_CC_Recipient, inboxMessage.EI_CC_Recipient);
			Assert.AreEqual(expectedMessage.EI_ApplicationCode, inboxMessage.EI_ApplicationCode);
			Assert.AreEqual(expectedMessage.EI_MessageType, inboxMessage.EI_MessageType);
			Assert.AreEqual(expectedMessage.EI_Content, inboxMessage.EI_Content);

			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleBusinessResponse_WhenConversationIdIsMissing_ShouldThrowHttpException()
		{
			var cspResponse = XDocument.Parse(GetMessage("MCP_BusinessResponse.xml"));
			cspResponse
				.Descendants("header")
				.Where(node => node
					.Attributes()
					.Any(attribute => attribute.Name == "name" && attribute.Value == "ConversationID"))
				.ToList()
				.ForEach(node => node.Remove());

			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCC",
				SubscriptionValue = "40ad9ef5-2d57-4286-a12a-31675ffe4d9b",
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = cspResponse.ToString(),
				MessageDoc = cspResponse,
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_BusinessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());
			Assert.AreEqual(501, exception.GetHttpCode());
			Assert.AreEqual("Provider: MCP. No Conversation ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.MCP);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
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

		private BusinessNotificationHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-MCP"},
			{"GBCustomsTestID", "GBCustomsTest-MCP"},
			{"GBCustomsRegistrationType", "GBCustoms-MCP"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionTypeByCSPID", "GBCMCP" },
			{"GBCustomsSubscriptionTypeByConversationID", "GBCMCC"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};

		public override string Provider => "MCP";
	}
}
