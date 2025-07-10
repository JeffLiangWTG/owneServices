using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class SyntaxAndServiceReportMessageProcessorTest : ResponseMessageProcessorTest
	{
		public void TestThreeManifestAcknowledgementsReceived()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+8+CONTRL:D:03B:UN
UCI+12+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCF+12+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCM+11+CUSCAR:D:03B:UN+7
UCM+13+CUSCAR:D:03B:UN+7
UCM+14+CUSCAR:D:03B:UN+7
UNT+7+8
UNZ+1+8
";
			CreateMessageAndExecuteBatch(interchangeText);
			//No associated transmit message has been found for first acknowledgement message
			var expectedBody = @"<b>There has been a problem processing the attached ACK (Acknowledgement) message.</b>
<br />
<br />
Error Details:<br />
<br />
No associated transmit message has been found that matches the following details:
Application Code 'MAN', Message Number '11'.";
			AssertMessageProcessed(message, "11", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.eManifest, Guid.Empty, Env.CurrentBranch.PK, Enterprise.Messaging.Business.EDIMessage.Status.Failed, expectedBody);
			AssertEquals("Notification for error has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail("e-Manifest Response Message Processor Error Report", expectedBody, message.EM_MessageText, string.Empty);
			//Second acknowledgement message has been processed successfully and attached to the job MAN0000001
			expectedBody = @"Reference Number : MAN0000001<br />
<br />
</strong>A Content Accepted response message has been received from CBP for a Complete e-Manifest w/ACE ID.<br />
The message has been validated and is error free.<br />";
			AssertEquals("One new message attached to the job MAN0000001", 4, trip1.Messages.Count);
			AssertMessageProcessed((EDIMessage)trip1.Messages[3], "13", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.eManifest, trip1.PK, branch2.PK, EDIMessage.Status.Received, expectedBody);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip1.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip1.BH_MessageStatus);
			//Third acknowledgement message has been processed successfully and attached to the job MAN0000002
			expectedBody = @"Reference Number : MAN0000002<br />
<br />
</strong>A Content Accepted response message has been received from CBP for a Complete e-Manifest w/ACE ID.<br />
The message has been validated and is error free.<br />";
			AssertEquals("One new message attached to the job MAN0000002", 2, trip2.Messages.Count);
			AssertMessageProcessed((EDIMessage)trip2.Messages[1], "14", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.eManifest, trip2.PK, branch1.PK, EDIMessage.Status.Received, expectedBody);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip2.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip2.BH_MessageStatus);
		}

		public void TestCrewAcknowledgementReceived()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+8+CONTRL:D:03B:UN
UCI+12+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCF+12+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCM+12+PAXLST:D:03B:UN+7
UNT+7+8
UNZ+1+8
";
			CreateMessageAndExecuteBatch(interchangeText);
			AssertEquals("No notifications has been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			const string expectedBody = @"Reference Number : MAN0000001<br />
<br />
</strong>A Content Accepted response message has been received from CBP for a Crew/Passengers Details.<br />
The message has been validated and is error free.<br />";
			AssertEquals("One new message attached to the job MAN0000001", 4, trip1.Messages.Count);
			AssertMessageProcessed((EDIMessage)trip1.Messages[3], "12", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.CrewAndPassenger, trip1.PK, branch1.PK, EDIMessage.Status.Received, expectedBody);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip1.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip1.BH_MessageStatus);
		}

		public void TestUnassociatedShipmentsAcknowledgementReceived()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+8+CONTRL:D:03B:UN
UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCM+15+CUSCAR:D:03B:UN+7
UNT+7+8
UNZ+1+8
";
			CreateMessageAndExecuteBatch(interchangeText);
			AssertEquals("No notifications has been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			const string expectedBody = @"Reference Number : MAN0000001<br />
<br />
</strong>A Content Accepted response message has been received from CBP for an Unassociated Shipments.<br />
The message has been validated and is error free.<br />";
			AssertEquals("One new message attached to the job MAN0000001", 4, trip1.Messages.Count);
			AssertMessageProcessed((EDIMessage)trip1.Messages[3], "15", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.UnassociatedShipments, trip1.PK, branch2.PK, EDIMessage.Status.Received, expectedBody);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip1.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip1.BH_MessageStatus);
		}

		public void TestManifestAcknowledgementAndSyntaxErrorReceived()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+17+CONTRL:D:03B:UN
UCI+14+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCF+14+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCM+14+CUSCAR:D:03B:UN+7
UCM+13+CUSCAR:D:03B:UN+4
UCS+5
UCD+13+2:1
UCD+12+2:1
UCS+6
UCD+12+2:1
UCS+8+15
UNT+11+17
UNZ+1+17
";
			CreateMessageAndExecuteBatch(interchangeText);
			//Acknowledgement message has been processed successfully and attached to the job MAN0000002
			var expectedBody = @"Reference Number : MAN0000002<br />
<br />
</strong>A Content Accepted response message has been received from CBP for a Complete e-Manifest w/ACE ID.<br />
The message has been validated and is error free.<br />";
			AssertEquals("One new message attached to the job MAN0000002", 2, trip2.Messages.Count);
			AssertMessageProcessed((EDIMessage)trip2.Messages[1], "14", MessageTypes.Codes.Acknowledgement, MessageTypes.Codes.eManifest, trip2.PK, branch1.PK, EDIMessage.Status.Received, expectedBody);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip2.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip2.BH_MessageStatus);
			//Syntax error message has been processed successfully and attached to the job MAN0000001
			expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CompleteManifestSyntaxErrorMessageInterpretation.html")
				.Replace("edient:Command=ShowEditForm&LicenceCode=&ControllerID=eManifest&BusinessEntityPK=84f89ebe-e700-4010-ade4-907fcba389e5&Hash=%2brjICa2iV3chJKHgGg04i1ii0j7YRz%2f9I", "");
			const string expectedSyntaxErrorText = @"1 - UNH+13+CUSCAR:D:03B:UN'
2 - BGM+85:::STANDARD+CWEBMAN0000004+22'
3 - DTM+132::203'
4 - LOC+60+:77'

{0} 5 - RFF+:MAN13'
{0} Error Message: Missing,  Component Value: '', Position: L: 5; P: 2,1.
{0} Error Message: Invalid value,  Component Value: '', Position: L: 5; P: 2,1.


{0} 6 - RFF+**:12317'
{0} Error Message: Invalid value,  Component Value: '**', Position: L: 6; P: 2,1.

7 - NAD+CA+CWEB:172'

{0} 8 - DOC'
{0} Error Message: Not supported in this position,  Component Value: '', Position: L: 8.

9 - UNT+9+13'";
			AssertEquals("One new message attached to the job MAN0000001", 4, trip1.Messages.Count);
			message = (EDIMessage)trip1.Messages[3];
			AssertSyntaxMessageProcessed(message, "13", MessageTypes.Codes.SyntaxError, MessageTypes.Codes.eManifest, trip1.PK, branch2.PK, EDIMessage.Status.Received, expectedBody, expectedSyntaxErrorText);
			AssertEquals("2 notification for syntax error has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertSyntaxErrorEmail("Syntax Error Complete e-Manifest w/ACE ID Response for MAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, string.Format(expectedSyntaxErrorText, "##"), "SecondUserToNotify@blah.com");
			AssertEquals("BH_ReleaseStatus", MessageTypes.Codes.SyntaxError, trip1.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip1.BH_MessageStatus);
		}

		public void TestTwoCrewMessageLevelSyntaxErrorsReceived()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+12+CONTRL:D:03B:UN
UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4
UCM+12+PAXLST:D:03B:UN+4+29
UCM+18+PAXLST:D:03B:UN+4+29
UNT+5+12
UNZ+1+8
";
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			CreateMessageAndExecuteBatch(interchangeText);
			AssertEquals("Notifications have been sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			//First syntax error message has been processed successfully and attached to the job MAN0000001, no job status changed if crew message
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewAndPassengersSyntaxErrorMessageInterpretation.html").Replace("edient:Command=ShowEditForm&LicenceCode=&ControllerID=eManifest&BusinessEntityPK=84f89ebe-e700-4010-ade4-907fcba389e5&Hash=%2brjICa2iV3chJKHgGg04i1ii0j7YRz%2f9I", "");
			const string expectedSyntaxErrorText = @"1 - UNH+12+PAXLST:D:03B:UN'
2 - BGM+10:::STANDARD+LOCKMAN0000001+4'
3 - RFF+CRW12'
4 - TDT+11++03++LOCK:172'
5 - DTM+132:20110826:102'
6 - UNT+24+12'

{0} Error Message: Control count does not match number of instances received";
			AssertEquals("One new message attached to the job MAN0000001", 4, trip1.Messages.Count);
			message = (EDIMessage)trip1.Messages[3];
			AssertSyntaxMessageProcessed(message, "12", MessageTypes.Codes.SyntaxError, MessageTypes.Codes.CrewAndPassenger, trip1.PK, branch1.PK, EDIMessage.Status.Received, expectedBody, expectedSyntaxErrorText);
			AssertSyntaxErrorEmail("Syntax Error Crew/Passengers Details Response for MAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, string.Format(expectedSyntaxErrorText, "##"));
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip1.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip1.BH_MessageStatus);
			//No associated transmit message has been found for second syntax error message
			expectedBody = @"<b>There has been a problem processing the attached SYN (Syntax Error) message.</b>
<br />
<br />
Error Details:<br />
<br />
No associated transmit message has been found that matches the following details:
Application Code 'MAN', Message Number '18'.";
			var query = new ZQuery(EDIMessageSchema.EM_MessageNum, "18");
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USeManifest);
			message = Factory.LoadTop1<EDIMessage>(query);
			AssertMessageProcessed(message, "18", MessageTypes.Codes.SyntaxError, string.Empty, Guid.Empty, Env.CurrentBranch.PK, EDIMessage.Status.Failed, expectedBody);
			AssertEmail("e-Manifest Response Message Processor Error Report", expectedBody, message.EM_MessageText, string.Empty);
		}

		public void TestInvalidSyntaxErrorMessage()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST'UNH+12+CONTRL:D:03B:UN'UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+4'UNT+5+12'UNZ+1+8'";
			const string errorMessage = "The message processor was unable to interpret received message.";
			CreateMessageAndExecuteBatch(interchangeText);
			AssertMessageProcessed(message, "12", MessageTypes.Codes.eManifest, MessageTypes.Codes.eManifest, Guid.Empty, Env.CurrentBranch.PK, EDIMessage.Status.Failed, errorMessage);
			AssertEquals("trip1.BH_ReleaseStatus", string.Empty, trip1.BH_ReleaseStatus);
			AssertEquals("trip1.BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip1.BH_MessageStatus);
			AssertEquals("trip2.BH_ReleaseStatus", string.Empty, trip2.BH_ReleaseStatus);
			AssertEquals("trip2.BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip2.BH_MessageStatus);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
			AssertEmail("e-Manifest Response Message Processor Error Report", errorMessage, message.EM_MessageText, string.Empty);
		}

		protected override IProcessorForTest GetProcessorForTest()
		{
			var processor = new ProcessForTest(logger);
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120213:0220+8++ACETEST
UNH+8+CONTRL:D:03B:UN
UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7
UCM+15+CUSCAR:D:03B:UN+7
UNT+7+8
UNZ+1+8
";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			message.EM_LinkedObject = trip1;
			Factory.Save();
			processor.ProcessMessage(message);
			return processor;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var userToNotify2 = Factory.NewWithValidTestData<GlbStaff>();
			userToNotify2.GS_FullName = "SecondUserToNotify";
			userToNotify2.GS_EmailAddress = "SecondUserToNotify@blah.com";
			userToNotify2.GS_Code = "NU2";
			branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch2 = Factory.NewWithValidTestData<GlbBranch>();
			trip1 = MessagingTestHelper.GetTripAwaitingReply(Factory, "84f89ebe-e700-4010-ade4-907fcba389e5", "MAN0000001");
			var text = @"UNH+<<MSGNO PLACEHOLDER>>+PAXLST:D:03B:UN'BGM+10:::STANDARD+LOCKMAN0000001+4'RFF+CRW12'TDT+11++03++LOCK:172'DTM+132:20110826:102'UNT+24+<<MSGNO PLACEHOLDER>>'";
			trip1.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, "12", userToNotify, branch1, ZDateTime.Now, text));
			text = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN'BGM+85:::STANDARD+CWEBMAN0000004+22'DTM+132::203'LOC+60+:77'RFF+:MAN13'RFF+**:12317'NAD+CA+CWEB:172'DOC'UNT+9+<<MSGNO PLACEHOLDER>>'";
			trip1.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "13", userToNotify2, branch2, ZDateTime.Now.AddDays(1), text));
			trip1.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "15", userToNotify2, branch2, ZDateTime.Now.AddDays(1)));
			trip2 = MessagingTestHelper.GetTripAwaitingReply(Factory, "5DB19F1B-0BB6-4110-A776-F43B460579C9", "MAN0000002");
			text = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN'BGM+85:::STANDARD+CWEBMAN0000004+22'DTM+132::203'LOC+60+:77'RFF+:MAN14'NAD+CA+CWEB:172'DOC'UNT+7+<<MSGNO PLACEHOLDER>>'";
			trip2.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "14", userToNotify, branch1, ZDateTime.Now.AddDays(2), text));
			processor = new MessageProcessor { Logger = logger };
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		Trip trip1;
		MessageProcessor processor;
		EDIMessage message;
		GlbBranch branch1;
		GlbBranch branch2;
		Trip trip2;
		EmbeddedResourceRetriever resourceRetriever;

		void AssertSyntaxMessageProcessed(EDIMessage ediMessage, string number, string type, string subType, ZGuid parentPk, ZGuid branchPk, string status, string expectedBody, string expectedSyntaxErrorText)
		{
			AssertMessageProcessed(ediMessage, number, type, subType, parentPk, branchPk, status, string.Empty);
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertContains("EM_FormattedMessageText", string.Format(expectedSyntaxErrorText, "##"), message.EM_FormattedMessageText);
		}

		void AssertMessageProcessed(EDIMessage ediMessage, string number, string type, string subType, ZGuid parentPk, ZGuid branchPk, string status, string expectedBody)
		{
			AssertEquals("EM_MessageNum", number, ediMessage.EM_MessageNum);
			AssertEquals("EM_MessageType", type, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", subType, ediMessage.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", parentPk, ediMessage.EM_LinkUniqueID);
			AssertEquals("EM_GB", branchPk, ediMessage.EM_GB);
			AssertEquals("EM_Status", status, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
		}

		void CreateMessageAndExecuteBatch(string interchangeText)
		{
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			processor.ExecuteBatch();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			trip1 = factory.Load<Trip>(trip1.PK);
			trip1.Messages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc);
			trip2 = factory.Load<Trip>(trip2.PK);
			trip2.Messages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc);
		}

		sealed class ProcessForTest : SyntaxAndServiceReportMessageProcessor, IProcessorForTest
		{
			public ProcessForTest(LoggingInformation logger) : base(logger)
			{
			}

			public new string AcknowledgementEmailMode => base.AcknowledgementEmailMode;
			public new string ImpedimentEmailMode => base.ImpedimentEmailMode;
			public new string ErrorEmailMode => base.ErrorEmailMode;
			public new EDIMessage originalMessage => base.originalMessage;
		}
	}
}
