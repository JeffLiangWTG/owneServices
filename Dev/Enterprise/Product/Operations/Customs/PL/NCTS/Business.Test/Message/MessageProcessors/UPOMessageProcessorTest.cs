using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(UPOMessageProcessor))]
sealed class UPOMessageProcessorTest : BaseNctsMessageProcessorTestCase<UPOMessageProcessor, IUpo>
{
	protected override string ExpectedMessageFriendlyName => $"{ApplicationCodes.PLCustomsNCTS}/{PUESC.SystemMessages.UPO}";

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(UPOMessageInterpreter);

	public void TestPreProcessMessage_Failed()
	{
		var message = Factory.New<EDIMessage>();
		MessageProcessor.PreProcessMessage(message);
		AssertEquals("Failed if linked object is not defined: EM_Status", expected: "FAL", message.EM_Status);
	}

	public void TestPreProcessMessage()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		Factory.Save();

		var testMessage = Factory.NewWithValidTestData<EDIMessage>();
		testMessage.EM_MessageNum = "TestNum";
		testMessage.EM_ApplicationCode = ApplicationCodes.PLCustomsNCTS;
		testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		testMessage.EM_Status = EDIMessage.Status.Sent;
		testMessage.EM_LinkedObject = nctsHeader.MovementHeader;
		testMessage.EM_MessageText = ZString.Empty;

		MessageProcessor.PreProcessMessage(testMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkTable", expected: "CusInBondMoveHeader", testMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", nctsHeader.MovementHeader.PK, testMessage.EM_LinkUniqueID);
		});
	}

	public void TestProcessMessage()
	{
		var user = Factory.New<GlbStaff>();
		user.GS_Code = "ABC";
		user.GS_LoginName = "ABC";
		user.GS_EmailAddress = "abc@abc.com";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.BH_JobReference = "TEST_HEADER_NUM";

		var testTransmittedMessage = Factory.New<EDIMessage>();
		testTransmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsNCTS;
		testTransmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		testTransmittedMessage.EM_MessageNum = "YYTTTTTMessageNum";
		testTransmittedMessage.EM_SystemCreateUser = "ABC";
		testTransmittedMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
		testTransmittedMessage.EM_Status = EDIMessage.Status.Sent;

		var testUpoMessage = Factory.New<EDIMessage>();
		testUpoMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
		testUpoMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		dataProviderMock.As<ICorrelationProvider>().SetupGet(x => x.CorrelationIdentifier).Returns("YYTTTTTMessageNum");
		MessageProcessor.ProcessMessage(testUpoMessage);

		var expectedSubject = $"UPO - Official Confirmation of Receipt (Type) YYTTTTTMessageNum. Response for {testNctsHeader.BH_JobReference}";
		var headerUri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(testNctsHeader);
		var expectedJobHRef = $"UPO - Official Confirmation of Receipt (Type) YYTTTTTMessageNum. Response for <a href=\"{headerUri}\">{testNctsHeader.BH_JobReference}</a>";

		CombineAssertions(() =>
		{
			AssertEquals("Message EM_Status", expected: "PRS", testUpoMessage.EM_Status);
			Assert("Message Has interpretation", !testUpoMessage.EM_MessageInterpretation.IsEmpty);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var expectedEmailAddress = user.GS_EmailAddress;
			AssertEquals("Email address", expectedEmailAddress, email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertEquals("Email subject", expectedSubject, email.Subject);
			AssertContains("Email body", testUpoMessage.EM_MessageInterpretation, email.Body);
			AssertContains("Job reference", expectedJobHRef, email.Body);
		});
	}

	public override void TestPreProcessMessage_CorrelationIdentifier() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_LRN() => Assert("Not Supported, custom GetLinkedObject", true);

	public override void TestPreProcessMessage_MRN() => Assert("Not Supported, custom GetLinkedObject", true);

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock.SetupGet(x => x.NameOfIssuingSystem).Returns("NameOfIssuingSystem");
		dataProviderMock.SetupGet(x => x.NameOfTheApplicant).Returns("NameOfTheApplicant");
		dataProviderMock.SetupGet(x => x.IdentifierEcipSeap).Returns("IdentifierEcipSeap");
		dataProviderMock.SetupGet(x => x.DateOfCreation).Returns("DateOfCreation");
		dataProviderMock.SetupGet(x => x.DateOfCompletion).Returns("DateOfCompletion");

		dataProviderMock.SetupGet(x => x.TransmitDocumentAbbreviated).Returns("Abbreviated");
		dataProviderMock.As<IExternalSystemIdProvider>().SetupGet(m => m.ExternalSystemID).Returns("SeapID");
		dataProviderMock.SetupGet(x => x.TransmitDocumentType).Returns("Type");

		var engContentMock = new Mock<ITextInLanguage>();
		engContentMock.SetupGet(x => x.Language).Returns("ENG");
		engContentMock.SetupGet(x => x.Text).Returns("English text");

		var plContentMock = new Mock<ITextInLanguage>();
		plContentMock.SetupGet(x => x.Language).Returns("PL");
		plContentMock.SetupGet(x => x.Text).Returns("Polish text");
	}
}
