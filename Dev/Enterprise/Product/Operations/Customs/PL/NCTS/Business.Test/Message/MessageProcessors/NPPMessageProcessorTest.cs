using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NPPMessageProcessor))]
sealed class NPPMessageProcessorTest : BaseNctsMessageProcessorTestCase<NPPMessageProcessor, IConfirmation>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.NPP;

	protected override bool ExpectedIsFailureNotification => true;

	protected override Type ExpectedMessageInterpreterType => typeof(NPPMessageInterpreter<NctsCommonMovementHeader>);

	public void TestProcessMessage()
	{
		const string expectedInterpretation =
			"<table border=\"0\">" +
			"<tr><th align=\"left\" width=\"800\">Komunikat NPP (Poświadczenie Nieprzedłożenia Dokumentu)</th></tr>" +
			"</table><hr /><br />" +
			"<table border=\"0\">" +
			"<tr><th align=\"left\" width=\"300\">IdentyfikatorPoswiadczenia</th><td align=\"left\" width=\"500\"> : TestExternalSystemID</td></tr>" +
			"<tr><th align=\"left\" width=\"300\">idDokumentuSEAP</th><td align=\"left\" width=\"500\"> : TEST_REF</td></tr>" +
			"<tr><th align=\"left\" width=\"300\">Przyczyna błędu</th></tr>" +
			"</table>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
			"<tr><td align=\"left\" width=\"800\">&nbsp;</td></tr>" +
			"</table><br /><hr />";

		var user = Factory.New<GlbStaff>();
		user.GS_Code = "ABC";
		user.GS_LoginName = "ABC";
		user.GS_EmailAddress = "abc@abc.com";

		dataProviderMock.Setup(x => x.ExternalSystemID).Returns("TestExternalSystemID");
		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns("TEST_REF");

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.BH_JobReference = "TEST_HEADER_NUM";

		var testTransmittedMessage = Factory.New<EDIMessage>();
		testTransmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsNCTS;
		testTransmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		testTransmittedMessage.EM_ApplicationReference = "TEST_REF";
		testTransmittedMessage.EM_SystemCreateUser = "ABC";
		testTransmittedMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
		testTransmittedMessage.EM_Status = EDIMessage.Status.Sent;

		var testNppMessage = Factory.New<EDIMessage>();
		testNppMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
		testNppMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		testNppMessage.EM_ApplicationReference = "TEST_REF";

		var expectedSubject = $"NPP Response (Failure) for {testNctsHeader.BH_JobReference}";
		var headerUri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(testNctsHeader);
		var expectedJobHRef = $"NPP Response (Failure) for <a href=\"{headerUri}\">{testNctsHeader.BH_JobReference}</a>";

		MessageProcessor.ProcessMessage(testNppMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Message EM_Status", expected: "PRS", testNppMessage.EM_Status);
			AssertEquals("Message EM_MessageInterpretation", expectedInterpretation, testNppMessage.EM_MessageInterpretation);
			AssertEquals("MovementHeader status BH_MessageStatus", expected: "ERR", testNctsHeader.BH_MessageStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var expectedEmailAddress = user.GS_EmailAddress;
			AssertEquals("Email address", expectedEmailAddress, email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertEquals("Email subject", expectedSubject, email.Subject);
			AssertContains("Email body", testNppMessage.EM_MessageInterpretation, email.Body);
			AssertContains("Job reference", expectedJobHRef, email.Body);
		});
	}

	public override void TestPreProcessMessage_CorrelationIdentifier() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_LRN() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_MRN() => Assert("Not Supported, custom GetLinkedObject", true);
}
