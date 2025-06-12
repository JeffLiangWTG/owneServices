using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MessageHelperTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageHelper_EnqueueTest()
		{
			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			var testRules = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRule>();

			var mockMessageHelper = MockRepository.GenerateMock<MessageHelper>();
			mockMessageHelper.Stub(x => x.GetDBContext()).Return(mockContext);

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.Default.GetBytes("<TestMsg><SCAC>BBBB</SCAC></TestMsg>"));
			message.Context.WriteProperty<BTS.SourceParty>("TESTSENDER__1");
			message.Context.WriteProperty<BTS.DestinationParty>("TESTRECIPIENT__1");
			string trackingID = "08e27e37-041d-490c-92ec-5b05eb47e1bf";
			message.Context.WriteProperty<MessageTrackingID>(trackingID);

			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(MessageFactory);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.SqlQuery<DateTime>("", new object[0])).IgnoreArguments().Return(new[] { DateTime.UtcNow });

			var messageQueue = new Queue();
			mockMessageHelper.EnqueueMessage(pipelineContext, messageQueue, message, message, false, false);

			var resultMessages = messageQueue.Cast<IBaseMessage>().ToArray();
			Assert.AreEqual(1, resultMessages.Count());
			Assert.AreSame(message, resultMessages[0]);
			Assert.AreEqual(trackingID, resultMessages[0].Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("TESTSENDER__1", resultMessages[0].Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("TESTRECIPIENT__1", resultMessages[0].Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageHelper_EnqueueProviderMappingTest()
		{
			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			var testRules = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRule>();
			var testFacts = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>();

			var mockMessageHelper = MockRepository.GenerateMock<MessageHelper>();
			mockMessageHelper.Stub(x => x.GetDBContext()).Return(mockContext);

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.Default.GetBytes("<TestMsg><SCAC>AAAA</SCAC></TestMsg>"));
			message.Context.WriteProperty<BTS.SourceParty>("TESTSENDER__1");
			message.Context.WriteProperty<BTS.DestinationParty>("SHIPPING_INSTRUCTION");
			string trackingID = "08e27e37-041d-490c-92ec-5b05eb47e1bf";
			message.Context.WriteProperty<MessageTrackingID>(trackingID);

			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(MessageFactory);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.SqlQuery<DateTime>("", new object[0])).IgnoreArguments().Return(new[] { DateTime.UtcNow });

			var clientInttra = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "INTTRA_SI" });
			var clientGTNexus = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "GTNEXUS" });
			var clientService = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "SHIPPING_INSTRUCTION", CC_RR = Guid.NewGuid() });
			var providerInttra = new eServices.eHubDataModel.eHubTransactions.eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
			var providerGTNexus = new eServices.eHubDataModel.eHubTransactions.eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientGTNexus };
			clientService.eHubRoutingRule = testRules.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]", eHubServiceProvider = providerGTNexus, RR_Group_Ordering = 1000 },
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule { RR_Condition_Expression = "[@SCAC,Equal,AAAA]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 2000 }
				}).ToList(),
				RR_Failed_ErrorCode = "IRJ",
				RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct",
				eHubRoutingRuleFacts = new List<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>
				{
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "SCAC", RX_Type = "XPATH", RX_Query = "//SCAC/text()" }),
				},
				RR_LastUpdateUTC = DateTime.UtcNow
			});
			clientService.eHubServiceProviders_Service = new List<eServices.eHubDataModel.eHubTransactions.eHubServiceProvider> { providerInttra, providerGTNexus };
			clientService.eHubRoutingRule.eHubClients.Add(clientService);

			var messageQueue = new Queue();
			mockMessageHelper.EnqueueMessage(pipelineContext, messageQueue, message, message, false, false);

			var resultMessages = messageQueue.Cast<IBaseMessage>().ToArray();
			Assert.AreEqual(2, resultMessages.Length);
			Assert.AreSame(message, resultMessages[0]);
			Assert.AreEqual(trackingID, resultMessages[0].Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreNotSame(message, resultMessages[1]);
			Assert.AreNotEqual(trackingID, resultMessages[1].Context.ReadPropertyString<MessageTrackingID>());
			Assert.IsTrue(resultMessages.All(m => m.Context.ReadPropertyString<BTS.SourceParty>() == "TESTSENDER__1"));
			Assert.AreEqual(1, resultMessages.Count(m => m.Context.ReadPropertyString<BTS.DestinationParty>() == "INTTRA_SI"));
			Assert.AreEqual(1, resultMessages.Count(m => m.Context.ReadPropertyString<BTS.DestinationParty>() == "GTNEXUS"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageHelper_EnqueueProviderMappingTest_Reject()
		{
			var mockMessageHelper = MockRepository.GenerateMock<MessageHelper>();
			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			var testRules = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRule>();
			var testFacts = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>();
			mockMessageHelper.Stub(x => x.GetDBContext()).Return(mockContext);
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.Default.GetBytes("<TestMsg><SCAC>BBBB</SCAC></TestMsg>"));
			message.Context.WriteProperty<BTS.SourceParty>("TESTSENDER__2");
			message.Context.WriteProperty<BTS.DestinationParty>("SHIPPING_INSTRUCTION");
			string trackingID = "08e27e37-041d-490c-92ec-5b05eb47e1bf";
			message.Context.WriteProperty<MessageTrackingID>(trackingID);
			var messageQueue = new Queue();
			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(MessageFactory);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.SqlQuery<DateTime>("", new object[0])).IgnoreArguments().Return(new[] { DateTime.UtcNow });
			var clientInttra = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "INTTRA_SI" });
			var clientGTNexus = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "GTNEXUS" });
			var clientService = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "SHIPPING_INSTRUCTION", CC_RR = Guid.NewGuid() });
			var providerInttra = new eServices.eHubDataModel.eHubTransactions.eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
			var providerGTNexus = new eServices.eHubDataModel.eHubTransactions.eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientGTNexus };
			clientService.eHubRoutingRule = testRules.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]", eHubServiceProvider = providerGTNexus, RR_Group_Ordering = 1000 },
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule { RR_Condition_Expression = "[@SCAC,Equal,AAAA]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 2000 }
				}).ToList(),
				RR_Failed_ErrorCode = "IRJ",
				RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct",
				eHubRoutingRuleFacts = new List<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>
				{
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "SCAC", RX_Type = "XPATHNAV", RX_Query = "//SCAC/text()" }),
				},
				RR_LastUpdateUTC = DateTime.UtcNow
			});
			clientService.eHubServiceProviders_Service = new List<eServices.eHubDataModel.eHubTransactions.eHubServiceProvider> { providerInttra, providerGTNexus };
			clientService.eHubRoutingRule.eHubClients.Add(clientService);

			mockMessageHelper.EnqueueMessage(pipelineContext, messageQueue, message, message, false, false);

			var resultMessages = messageQueue.Cast<IBaseMessage>().ToArray();
			Assert.AreEqual(1, resultMessages.Count());
			Assert.AreSame(message, resultMessages[0]);
			Assert.AreEqual(trackingID, resultMessages[0].Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("SHIPPING_INSTRUCTION", resultMessages[0].Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("TESTSENDER__2", resultMessages[0].Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("IRJ", resultMessages[0].Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct", resultMessages[0].Context.ReadPropertyString<ErrorDescription>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageHelper_EnqueueProviderMappingTest_NoRoute()
		{
			var mockMessageHelper = MockRepository.GenerateMock<MessageHelper>();
			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			var testRules = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRule>();
			mockMessageHelper.Stub(x => x.GetDBContext()).Return(mockContext);
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.Default.GetBytes("<TestMsg><SCAC>BBBB</SCAC></TestMsg>"));
			message.Context.WriteProperty<BTS.SourceParty>("TESTSENDER__4");
			message.Context.WriteProperty<BTS.DestinationParty>("INTRA_TI");
			string trackingID = "18e27e37-041d-490c-92ec-1b05eb47e1ba";
			message.Context.WriteProperty<MessageTrackingID>(trackingID);
			var messageQueue = new Queue();
			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(MessageFactory);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.SqlQuery<DateTime>("", new object[0])).IgnoreArguments().Return(new[] { DateTime.UtcNow });
			var clientService = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient()
			{
				CC_ID = "INTRA_TI",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = testRules.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule
				{
					RR_Group_MatchMultiple = true,
					RR_LastUpdateUTC = DateTime.UtcNow
				})
			});
			clientService.eHubRoutingRule.eHubClients.Add(clientService);

			mockMessageHelper.EnqueueMessage(pipelineContext, messageQueue, message, message, false, true);

			Assert.AreEqual(0, messageQueue.Count);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageHelper_EnqueueTest_FailMessageWhenRecipientIsError()
		{
			var mockMessageHelper = MockRepository.GenerateMock<MessageHelper>();
			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			var testRules = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRule>();
			var testFacts = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>();
			mockMessageHelper.Stub(x => x.GetDBContext()).Return(mockContext);
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.Default.GetBytes("<TestMsg><SCAC>BBBB</SCAC></TestMsg>"));
			message.Context.WriteProperty<BTS.SourceParty>("TESTSENDER__2");
			message.Context.WriteProperty<BTS.DestinationParty>("JPC");
			string trackingID = "08e27e37-041d-490c-92ec-5b05eb47e1bf";
			message.Context.WriteProperty<MessageTrackingID>(trackingID);
			var messageQueue = new Queue();
			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(MessageFactory);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.SqlQuery<string>("", new object[0])).IgnoreArguments().Return(new List<string>());
			mockContext.Stub(x => x.SqlQuery<DateTime>("", new object[0])).IgnoreArguments().Return(new[] { DateTime.UtcNow });
			var clientService = testClients.Add(new eServices.eHubDataModel.eHubTransactions.eHubClient { CC_ID = "JPC", CC_RR = Guid.NewGuid() });
			clientService.eHubRoutingRule = testRules.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				RR_Success_CC_Recipient = clientService.CC_PK,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule {RR_Result_Value = "ERROR", RR_Condition_Expression = "[@MessageReference,Equal,]", RR_Failed_ErrorCode = "IRJ", RR_Group_Ordering = 1000, RR_Failed_ErrorDescription = "Department=WiseTechGlobal|Reason=Could not find MessageReference in registration information. Contact WTG to register." },
					new eServices.eHubDataModel.eHubTransactions.eHubRoutingRule {RR_Result_Value = "ERROR", RR_Condition_Expression = "[@Password,Equal,]", RR_Failed_ErrorCode = "IRJ", RR_Group_Ordering = 2000, RR_Failed_ErrorDescription = "Department=WiseTechGlobal|Reason=Could not find Password in registration information. Contact WTG to register." }
				}).ToList(),
				eHubRoutingRuleFacts = new List<eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact>
				{
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "DestinationParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#DestinationParty" }),
							testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "MessageReference", RX_Type = "SQL", RX_Query = "SELECT CR_MessageReference FROM eHubMessageReferenceRegistry JOIN eHubClient ON CR_CC_Client = CC_PK WHERE CC_ID = @SourceParty AND CR_ApplicationCode = 'JPC'" }),
							testFacts.Add(new eServices.eHubDataModel.eHubTransactions.eHubRoutingRuleFact { RX_Name = "Password", RX_Type = "SQL", RX_Query = "SELECT CR_Password FROM eHubMessageReferenceRegistry JOIN eHubClient ON CR_CC_Client = CC_PK WHERE CC_ID = @SourceParty AND CR_ApplicationCode = 'JPC'" }),
				},
				RR_LastUpdateUTC = DateTime.UtcNow
			});
			clientService.eHubRoutingRule.eHubClients.Add(clientService);

			mockMessageHelper.EnqueueMessage(pipelineContext, messageQueue, message, message, false, false);

			var resultMessages = messageQueue.Cast<IBaseMessage>().ToArray();
			Assert.AreEqual(1, resultMessages.Count());
			Assert.AreSame(message, resultMessages[0]);
			Assert.AreEqual(trackingID, resultMessages[0].Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("TESTSENDER__2", resultMessages[0].Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("JPC", resultMessages[0].Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("IRJ", resultMessages[0].Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Department=WiseTechGlobal|Reason=Could not find MessageReference in registration information. Contact WTG to register.", resultMessages[0].Context.ReadPropertyString<ErrorDescription>());
		}
	}
}
