using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Products.NZCustoms.Client;
using CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1;
using CargoWise.eHub.Products.NZCustoms.Common;
using CargoWise.eHub.Products.NZCustoms.PullService;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	[TestClass]
	public class NZCustomsPullManagerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[Ignore]
		public void NZCustomsPullManagerTests_SendMessageToBiztalk_Integration()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.IntergrationTest = true;

			var attachments = new[]
            {
                new Attachment() {Filename = "filename1.txt", ContentType = @"text\xml", Content = "Attachment 1"},
                new Attachment() {Filename = "filename2.txt", ContentType = @"text\xml", Content = "QXR0YWNobWVudCBUZXh0"},
                new Attachment() {Filename = "filename3.txt", ContentType = @"text\xml", Content = "Attachment 3"}
            };

			var pullResult = new PullResult("Customer1", "MR10001", "filename2.txt", attachments);

			Assert.AreEqual(true, manager.SendMessageToBiztalkTest(pullResult));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessageAndPushItToBiztalk_PullResultEmpty()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var serviceApi = new TestServiceAPI();
			var fileManager = new TestFileManager();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PullNZCustomsServiceForNewMessageReturn = null;
			Assert.AreEqual(false, manager.PullNZCustomsServiceForNewMessageAndPushItToBiztalk());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceFoundNoMessages()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "PreviousMailId";
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.IsTrue(string.IsNullOrEmpty(((TestLogger)manager.InvalidResponseLogger).Log));
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId 'PreviousMailId'.
Info - Found no messages using MailboxMsgId [PreviousMailId]", logger.Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessageAndPushItToBiztalk_Send()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var serviceApi = new TestServiceAPI();
			var fileManager = new TestFileManager();

			var attachments = new[]
            {
                new Attachment() {Filename = "filename1.txt", ContentType = @"text\xml", Content = "Attachment 1"}
            };

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = new PullResult("Customer1", "Message1", "", attachments);
			manager.PullNZCustomsServiceForNewMessageReturn = pullResult;
			manager.SendMessageToBiztalkReturn = true;
			Assert.AreEqual(true, manager.PullNZCustomsServiceForNewMessageAndPushItToBiztalk());
			Assert.AreEqual(@"Info - Processed message successfully.", logger.Log.Trim());

			manager.SendMessageToBiztalkReturn = false;
			Assert.AreEqual(false, manager.PullNZCustomsServiceForNewMessageAndPushItToBiztalk());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_CheckRequest()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = null;

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "PreviousMailId";
			manager.PullNZCustomsServiceForNewMessageTest();

			Assert.IsNotNull(serviceApi.RequestLodgeResponseRequestTest);
			Assert.AreEqual("CargoWise", serviceApi.RequestLodgeResponseRequestTest.Submitter);
			Assert.AreEqual("PreviousMailId", serviceApi.RequestLodgeResponseRequestTest.MailboxMsgId);
			Assert.AreEqual(@"Warn - Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is [PreviousMailId]. RequestLodgeResponseResponse is null.", ((TestLogger)manager.InvalidResponseLogger).Log.Trim());
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId 'PreviousMailId'.", logger.Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_ResponseNull()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = null;

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.AreEqual(@"Warn - Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is []. RequestLodgeResponseResponse is null.", ((TestLogger)manager.InvalidResponseLogger).Log.Trim());
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId ''.", logger.Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_RequestResponseResponseNull()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.AreEqual(@"Warn - Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is []. RequestResponseResponse is null", ((TestLogger)manager.InvalidResponseLogger).Log.Trim());
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId ''.", logger.Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_AttachmentNull()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.attachments = null;
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "CustomerReference1_FileName.txt";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "PartnerId1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "MessageRefernece1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.AreEqual(@"Warn - Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is []. MailboxMsgId=MailBoxId1, RequestResponseResponse=CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1.RequestResponseResponse, MessageName=CustomerReference1_FileName.txt, Partner=PartnerId1, CustomerReference=MessageRefernece1, Attachments:",
				((TestLogger)manager.InvalidResponseLogger).Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_AttachmentValueNull()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.attachments = new Attachment[0];
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "CustomerReference1_FileName.txt";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "PartnerId1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "MessageRefernece1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "PreviousMailId";
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.AreEqual(@"Warn - Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is [PreviousMailId]. MailboxMsgId=MailBoxId1, RequestResponseResponse=CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1.RequestResponseResponse, MessageName=CustomerReference1_FileName.txt, Partner=PartnerId1, CustomerReference=MessageRefernece1, Attachments:",
				((TestLogger)manager.InvalidResponseLogger).Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_AttachmentValueWithBrackets()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "CustomerReference1_FileName.txt";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "PartnerId1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "MessageRefernece1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";

			var attachments = new List<Attachment>
			{
				new Attachment() {Filename = "<FileName1.txt>", ContentType = @"text\xml", Content = "Attachement1"},
				new Attachment() {Filename = "CustomerReference1_FileName.txt", ContentType = @"text\xml", Content = "Attachement2"}
			};
			serviceApi.RequestLodgeResponseResponseTest.attachments = attachments.ToArray();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(PullResult.PullStatus.MessageReceived, pullResult.Status);
			Assert.AreEqual("CustomerReference1", pullResult.CustomerReference);
			Assert.AreEqual("MessageRefernece1", pullResult.MessageReference);
			Assert.AreEqual(1, pullResult.Attachments.Length);
			Assert.AreEqual("Attachement2", pullResult.Content);
			Assert.AreEqual("FileName1.txt", pullResult.Attachments[0].Filename);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_Exception()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			Func<RequestLodgeResponseRequest, RequestLodgeResponseResponse> action = request => { throw new FaultException("Some error"); };
			var serviceApi = new TestServiceAPI(action);

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId ''.
Warn - Exception thrown during message pulling. Exception message: Some error
", logger.Log);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "CustomerReference1_FileName.txt";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "PartnerId1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "MessageRefernece1";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";

			var attachments = new List<Attachment>
            {
                new Attachment() {Filename = "filename1.txt", ContentType = @"text\xml", Content = "Attachment 1"}
            };
			serviceApi.RequestLodgeResponseResponseTest.attachments = attachments.ToArray();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(PullResult.PullStatus.MessageReceived, pullResult.Status);
			Assert.AreEqual("CustomerReference1", pullResult.CustomerReference);
			Assert.AreEqual("MessageRefernece1", pullResult.MessageReference);
			Assert.AreEqual(0, pullResult.Attachments.Length);
			Assert.AreEqual("Attachment 1", pullResult.Content);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_PullNZCustomsServiceForNewMessage_StatusNotFound()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();

			var attachments = new List<Attachment>
            {
                new Attachment() {Filename = "StatusNotFound", ContentType = @"application/octet-stream", Content = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+PE1lc3NhZ2VJbmZvPg0KPFN1Ym1pdHRlcj5DQVJHT1dJU0U8L1N1Ym1pdHRlcj4NCjxNc2dJRD4xMzQ1NzwvTXNnSUQ+DQo8UmVxU3RhdHVzPk5PVCBGT1VORDwvUmVxU3RhdHVzPg0KPEFja1N0YXR1cz5PSzwvQWNrU3RhdHVzPg0KPE1haWxib3hNc2dJZC8+DQo8RW50cnlJRC8+DQo8Q3VzdG9tZXJSZWZlcmVuY2UvPg0KPERvY3VtZW50VHlwZS8+DQo8UGFydG5lci8+DQo8L01lc3NhZ2VJbmZvPg=="}
            };
			serviceApi.RequestLodgeResponseResponseTest.attachments = attachments.ToArray();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "PreviousMailId";
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(null, pullResult);
			Assert.IsTrue(string.IsNullOrEmpty(((TestLogger)manager.InvalidResponseLogger).Log));
			Assert.AreEqual(@"Debug - Requesting lodgement response for MailboxMsgId 'PreviousMailId'.
Info - Found no messages using MailboxMsgId [PreviousMailId]", logger.Log.Trim());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_SendMessageToBiztalk()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);

			var pullResult = new PullResult("Customer1", "MR100101", "", new[] { new Attachment() { Content = "QXR0YWNobWVudCBUZXh0", ContentType = "type", Filename = "filename1" } });
			Assert.AreEqual(true, manager.SendMessageToBiztalkTest(pullResult));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_SendMessageToBiztalk_GeneralException()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.CreateChannelAndSendMessageException = "Some Error";

			var attachments = new[]
            {
                new Attachment() {Filename = "filename2.txt", ContentType = @"text\xml", Content = "QXR0YWNobWVudCBUZXh0"},
            };

			var pullResult = new PullResult("Customer1", "MR101001", "", attachments);

			Assert.AreEqual(false, manager.SendMessageToBiztalkTest(pullResult));

			Assert.AreEqual(@"Debug - Sending to BizTalk message with CustomerReference: Customer1 and MessageReference: MR101001
Warn - Unable to send message with CustomerReference = Customer1 to BizTalk. Exception message: Some Error
", logger.Log);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_SendMessageToBiztalk_MessageSpecificExceptionException()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.CreateChannelAndSendMessageException = "Bla bla bla Message specific error bla bla bla";

			var attachments = new[]
            {
                new Attachment() {Filename = "filename2.txt", ContentType = @"text\xml", Content = "QXR0YWNobWVudCBUZXh0"},
            };

			var pullResult = new PullResult("Customer1", "MR101001", "", attachments);

			Assert.AreEqual(true, manager.SendMessageToBiztalkTest(pullResult));

			Assert.AreEqual(@"Debug - Sending to BizTalk message with CustomerReference: Customer1 and MessageReference: MR101001
Warn - Unable to send message with CustomerReference = Customer1 to BizTalk. Exception message match 'Message specific error' pattern from configuration. Message saved into fileName1 and removed from queue. Please fix the error and message will be reprocessed automatically. Exception message: Bla bla bla Message specific error bla bla bla
", logger.Log);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_SkipMessageWhenResponseIsInvalid()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "InvalidName.txt";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "MailBoxId0";
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.IsNull(pullResult);
			Assert.AreEqual("MailBoxId1", manager.PreviousMailboxMsgIdTest);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsPullManagerTests_GetMessagePropertiesFromMessageName()
		{
			var logger = new TestLogger();
			var configuration = new TestConfigurationProvider();
			var fileManager = new TestFileManager();
			var serviceApi = new TestServiceAPI();

			serviceApi.RequestLodgeResponseResponseTest = new Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse = new Client.RequestLodgeResponse_v1.RequestResponseResponse();
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MessageName = "40045816C_54874193.xml";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.Partner = "";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.CustomerReference = "";
			serviceApi.RequestLodgeResponseResponseTest.RequestResponseResponse.MailboxMsgId = "MailBoxId1";
			var attachments = new List<Attachment>
			{
				new Attachment() {Filename = "filename1.txt", ContentType = @"text\xml", Content = "Attachment 1"}
			};
			serviceApi.RequestLodgeResponseResponseTest.attachments = attachments.ToArray();

			var manager = new NZCustomsPullManagerTest(logger, configuration, serviceApi, fileManager);
			manager.PreviousMailboxMsgIdTest = "MailBoxId0";
			var pullResult = manager.PullNZCustomsServiceForNewMessageTest();

			Assert.AreEqual(PullResult.PullStatus.MessageReceived, pullResult.Status);
			Assert.AreEqual("40045816C", pullResult.CustomerReference);
			Assert.AreEqual("54874193", pullResult.MessageReference);
			Assert.AreEqual("MailBoxId1", manager.PreviousMailboxMsgIdTest);
		}

		public class NZCustomsPullManagerTest : NZCustomsPullManager
		{
			public NZCustomsPullManagerTest(ILog logger, IConfigurationProvider configuration, IServiceApi api, IFileManager fileMananger)
				: base(logger, configuration, api, fileMananger)
			{
				IntergrationTest = false;
			}

			public PullResult PullNZCustomsServiceForNewMessageReturn { get; set; }

			public bool? SendMessageToBiztalkReturn { get; set; }

			public string CreateChannelAndSendMessageException { get; set; }

			public bool IntergrationTest { get; set; }

			protected override PullResult PullNZCustomsServiceForNewMessage()
			{
				return PullNZCustomsServiceForNewMessageReturn ?? base.PullNZCustomsServiceForNewMessage();
			}

			protected override bool SendMessageToBiztalk(PullResult pullResult)
			{
				return !SendMessageToBiztalkReturn.HasValue ? base.SendMessageToBiztalk(pullResult) : SendMessageToBiztalkReturn.Value;
			}

			public PullResult PullNZCustomsServiceForNewMessageTest()
			{
				PullNZCustomsServiceForNewMessageReturn = null;
				return PullNZCustomsServiceForNewMessage();
			}

			public bool SendMessageToBiztalkTest(PullResult pullResult)
			{
				SendMessageToBiztalkReturn = null;
				return SendMessageToBiztalk(pullResult);
			}

			public string PreviousMailboxMsgIdTest
			{
				get
				{
					return PreviousMailboxMsgId;
				}
				set
				{
					PreviousMailboxMsgId = value;
				}
			}

			protected override void CreateChannelAndSendMessage(Common.NZCustomsReply message)
			{
				if (!string.IsNullOrEmpty(CreateChannelAndSendMessageException)) throw new Exception(CreateChannelAndSendMessageException);
				if (IntergrationTest) base.CreateChannelAndSendMessage(message);
			}

			public override ILog InvalidResponseLogger
			{
				get
				{
					return invalidResponseLogger;
				}
			}

			readonly TestLogger invalidResponseLogger = new TestLogger();
		}
	}
}
