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

[TestedType(typeof(UPPMessageProcessor))]
sealed class UPPMessageProcessorTest : BaseNctsMessageProcessorTestCase<UPPMessageProcessor, IConfirmation>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.UPP;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(ConfirmationMessageInterpreter<NctsCommonMovementHeader>);

	public void TestProcessMessage()
	{
		const string expectedInterpretation = "UPP - Official Confirmation of Submission</br>IdentyfikatorPoswiadczenia: TestExternalSystemID</br>idDokumentuSEAP: TEST_REF";

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

		var testUppMessage = Factory.New<EDIMessage>();
		testUppMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
		testUppMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		testUppMessage.EM_ApplicationReference = "TEST_REF";

		var expectedSubject = $"UPP Response for {testNctsHeader.BH_JobReference}";
		var headerUri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(testNctsHeader);
		var expectedJobHRef = $"UPP Response for <a href=\"{headerUri}\">{testNctsHeader.BH_JobReference}</a>";

		MessageProcessor.ProcessMessage(testUppMessage);

		CombineAssertions(() =>
		{
			AssertEquals("Message EM_Status", expected: "PRS", testUppMessage.EM_Status);
			AssertEquals("Message EM_MessageInterpretation", expectedInterpretation, testUppMessage.EM_MessageInterpretation);
			AssertEquals("MovementHeader status BH_MessageStatus", expected: "ACC", testNctsHeader.BH_MessageStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var expectedEmailAddress = user.GS_EmailAddress;
			AssertEquals("Email address", expectedEmailAddress, email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertEquals("Email subject", expectedSubject, email.Subject);
			AssertContains("Email body", testUppMessage.EM_MessageInterpretation, email.Body);
			AssertContains("Job reference", expectedJobHRef, email.Body);
		});
	}

	public override void TestPreProcessMessage_CorrelationIdentifier() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_LRN() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_MRN() => Assert("Not Supported, custom GetLinkedObject", true);
}
