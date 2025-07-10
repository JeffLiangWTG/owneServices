using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD.Testing
{
	[TestedType(typeof(MessageSenderService))]
	sealed class MessageSenderServiceTest : ServiceTaskTestCase<MessageSenderService>
	{
		[TestDate(2021, 01, 20)]
		public void TestSendFromMultipleCompanies()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = company3.PK;

			Factory.Save();

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00001234Z");
			var mockMessage1 = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage1.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message1 = mockMessage1.Object;
			message1.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message1.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message1.EM_MessageType = MessageTypeList.Codes.CRE;
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1""><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>CRE</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>";
			message1.EM_GB = branch1.PK;
			Factory.Save();
			AssertEquals("PreCondition: message1.EM_Status", NZCMessage.Status.Queued, message1.EM_Status);

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch2.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00005678A");
			var mockMessage2 = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage2.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message2 = mockMessage2.Object;
			message2.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message2.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message2.EM_MessageType = MessageTypeList.Codes.CRE;
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1""><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>CRE</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>";
			message2.EM_GB = branch2.PK;
			Factory.Save();
			AssertEquals("PreCondition: message2.EM_Status", NZCMessage.Status.Queued, message2.EM_Status);

			var mockMessage3 = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage3.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message3 = mockMessage3.Object;
			message3.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message3.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message3.EM_MessageType = MessageTypeList.Codes.CRE;
			message3.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1""><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>CRE</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>";
			message3.EM_GB = branch3.PK;

			var mockMessage4 = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage4.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message4 = mockMessage4.Object;
			message4.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message4.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message4.EM_MessageType = MessageTypeList.Codes.CRE;
			message4.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1""><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>CRE</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>";
			message4.EM_GB = branch3.PK;
			message4.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-200);

			Factory.Save();
			AssertEquals("PreCondition: message3.EM_Status", NZCMessage.Status.Queued, message3.EM_Status);
			AssertEquals("PreCondition: message4.EM_Status", NZCMessage.Status.Queued, message4.EM_Status);

			var logger = new LoggerForTesting();
			var serviceTask = new MessageSenderService();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			message1.Reload();
			AssertEquals(NZCMessage.Status.Sent, message1.EM_Status);

			message2.Reload();
			AssertEquals(NZCMessage.Status.Sent, message2.EM_Status);

			message3.Reload();
			AssertEquals("Should be ignored as its create time is today.", NZCMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertEquals("Should be FAL as its create time is before 180 days than today.", NZCMessage.Status.Failed, message4.EM_Status);

			message1.Reload();
			AssertEquals(NZCMessage.Status.Sent, message1.EM_Status);
			var interchange1 = message1.Interchange;
			AssertEquals(EDIInterchange.Direction.Transmit, interchange1.EI_ReceiveTransmit);
			AssertEquals("TSW used eHub to send interchanges - status should be eHub queued", EDIInterchange.Status.eHubQueued, interchange1.EI_Status);
			AssertEquals(EDIMessage.ApplicationCodes.NewZealandCustoms, interchange1.EI_ApplicationCode);
			AssertEquals(branch1.PK, interchange1.EI_GB);

			message2.Reload();
			AssertEquals(NZCMessage.Status.Sent, message2.EM_Status);
			var interchange2 = message2.Interchange;
			AssertEquals(EDIInterchange.Direction.Transmit, interchange2.EI_ReceiveTransmit);
			AssertEquals("TSW used eHub to send interchanges - status should be eHub queued", EDIInterchange.Status.eHubQueued, interchange2.EI_Status);
			AssertEquals(EDIMessage.ApplicationCodes.NewZealandCustoms, interchange2.EI_ApplicationCode);
			AssertEquals(branch2.PK, interchange2.EI_GB);
		}

		public void TestSendHeldMessage()
		{
			var branch = SetupValidEnvironment();
			var mockMessage = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mockMessage.Object;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(5);
			message.EM_MessageType = MessageTypeList.Codes.CRE;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1""><WCODataModelVersion>3.2</WCODataModelVersion><WCODocumentName>CRE</WCODocumentName><CountryCode>NZ</CountryCode><AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>";
			message.EM_GB = branch.PK;
			Factory.Save();

			AssertEquals("PreCondition: message.EM_Status", NZCMessage.Status.Queued, message.EM_Status);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var logger = new LoggerForTesting();
			var serviceTask = new MessageSenderService();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			message.Reload();
			AssertEquals("held message is not processed", NZCMessage.Status.Queued, message.EM_Status);
			AssertNull(message.Interchange);

			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(-5);
			Factory.Save();

			serviceTask.RunTask();

			message.Reload();
			AssertEquals("message is processed", NZCMessage.Status.Sent, message.EM_Status);
			var interchange = message.Interchange;
			AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("TSW used eHub to send interchanges - status should be eHub queued", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIMessage.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
		}

		public void TestSendsAllQueuedMessagesEachCycle()
		{
			var branch = SetupValidEnvironment();

			var testMessage1 = CreateTestTSWMessage(branch.PK);
			var testMessage2 = CreateTestLegacyMessage(branch.PK);
			var testMessage3 = CreateTestTSWMessage(branch.PK);
			var testMessage4 = CreateTestTSWMessage(branch.PK);
			var testMessage5 = CreateTestTSWMessage(branch.PK);
			var testMessage6 = CreateTestLegacyMessage(branch.PK);
			Factory.Save();

			AssertEquals("PreCondition: testMessage1.EM_Status", NZCMessage.Status.Queued, testMessage1.EM_Status);
			AssertEquals("PreCondition: testMessage2.EM_Status", NZCMessage.Status.Queued, testMessage2.EM_Status);
			AssertEquals("PreCondition: testMessage3.EM_Status", NZCMessage.Status.Queued, testMessage3.EM_Status);
			AssertEquals("PreCondition: testMessage4.EM_Status", NZCMessage.Status.Queued, testMessage4.EM_Status);
			AssertEquals("PreCondition: testMessage5.EM_Status", NZCMessage.Status.Queued, testMessage5.EM_Status);
			AssertEquals("PreCondition: testMessage6.EM_Status", NZCMessage.Status.Queued, testMessage6.EM_Status);

			AssertEquals("PreCondition: interchange count", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var logger = new LoggerForTesting();
			var serviceTask = new MessageSenderService();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			AssertMessageWasSent(testMessage1);
			AssertMessageWasSent(testMessage2);
			AssertMessageWasSent(testMessage3);
			AssertMessageWasSent(testMessage4);
			AssertMessageWasSent(testMessage5);
			AssertMessageWasSent(testMessage6);

			AssertAllMessagesAreInSeperateInterchange();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"NZ Customs Messages outbound",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NewZealandCustoms),
				};
			}
		}

		EDIMessage CreateTestTSWMessage(ZGuid branchPK)
		{
			var mockMessage = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message = mockMessage.Object;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_MessageType = MessageTypeList.Codes.CRE;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf - 8""?><DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""<Hello World>";
			message.EM_GB = branchPK;
			return message;
		}

		EDIMessage CreateTestLegacyMessage(ZGuid branchPK)
		{
			var mockMessage = Factory.NewMoq<NZCMessageDummyForTest>();
			mockMessage.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message = mockMessage.Object;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_MessageType = "DEC";
			message.EM_MessageText = "UNH+:Hello World";
			message.EM_GB = branchPK;
			return message;
		}

		void AssertMessageWasSent(EDIMessage testMessage)
		{
			testMessage.Reload();
			if (testMessage.EM_MessageType == MessageTypeList.Codes.CRE)
			{
				AssertEquals("message.EM_Status", NZCMessage.Status.Sent, testMessage.EM_Status);
				var interchange = testMessage.Interchange;
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals(EDIMessage.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
				AssertEquals("TSW interchange is sent via eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			}
			else
			{
				AssertEquals("Legacy messages are no longer send - for testing should remain in QUE state", NZCMessage.Status.Queued, testMessage.EM_Status);
			}
		}

		void AssertAllMessagesAreInSeperateInterchange()
		{
			// TSW & Legacy messages cannot be created in the same InterchangeProvider. 
			// TSW messages do not want Header and Footer EDIFACT values populated but also need to use EI_InterchangeFooter to pass the MAC when required to eServices for inclusion in SOAP transmission wrapper.
			AssertEquals("There should be 4 interchanges created for the 4 TSW messages", 4, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
			foreach (EDIMessage testMessage in messagesCreated)
			{
				var messageInterchange = testMessage.Interchange;
				if (messageInterchange != null)
				{
					AssertEquals("Header for TSW xml interchange should be empty", ZString.Empty, messageInterchange.EI_HeaderText);
					AssertEquals("Footer for TSW xml interchange should be empty", ZString.Empty, messageInterchange.EI_FooterText);
					AssertEquals("Body should contain the message xml message text", testMessage.EM_MessageText, messageInterchange.EI_BodyText);
				}
			}
		}

		GlbBranch SetupValidEnvironment()
		{
			Env.Registry.MailServer = "Something";
			Env.Registry.MailboxUserName = "Something";
			Env.Registry.MailboxPassword = "Something";
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "AAA";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var branch = company.Branches.AddNew();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00001234Z");
			Factory.Save();

			return branch;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			Env.Registry.MailServer = "Something";
			Env.Registry.MailboxUserName = "Something";
			Env.Registry.MailboxPassword = "Something";
		}
	}

	public class NZCMessageDummyForTest : NZCMessage
	{
		public NZCMessageDummyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
		}
	}
}
