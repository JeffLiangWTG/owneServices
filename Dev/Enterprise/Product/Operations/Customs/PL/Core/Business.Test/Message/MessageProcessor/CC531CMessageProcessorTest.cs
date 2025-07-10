using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC531CMessageProcessor))]
sealed class CC531CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC531CMessageProcessor, ICC531C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC531;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC531CMessageInterpreter);

	public void TestProcessMessage()
	{
		var testDateTime = new DateTime(2024, 12, 23);
		var timerExpiryForSupplementaryDeclarationMock = new Mock<ICC531CTimerExpiryForSupplementaryDeclaration>();

		const string testTimerExpiryInformation = "TEST_TimerExpiryInformation";
		const string testMrn = "TST_MRN";
		const string testCustomsOfficeOfExportReferenceNumber = "TEST_CustomsOfficeOfExportReferenceNumber";
		dataProviderMock.Setup(x => x.TimerExpiryForSupplementaryDeclaration).Returns(timerExpiryForSupplementaryDeclarationMock.Object);
		timerExpiryForSupplementaryDeclarationMock.Setup(x => x.LodgementOfSupplementaryDeclarationStartDate).Returns(testDateTime);
		timerExpiryForSupplementaryDeclarationMock.Setup(x => x.LodgementOfSupplementaryDeclarationExpiryDate).Returns(testDateTime);
		timerExpiryForSupplementaryDeclarationMock.Setup(x => x.TimerExpiryInformation).Returns(testTimerExpiryInformation);

		var testEntryHeader = Factory.New<CusEntryHeader>();
		var message = Factory.New<EDIMessage>();
		const string expectedMessageStatus = EDIMessageStatus.ProcessedOK;
		message.EM_LinkedObject = testEntryHeader;
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		dataProviderMock.Setup(x => x.CustomsOfficeOfExportReferenceNumber).Returns(testCustomsOfficeOfExportReferenceNumber);

		MessageProcessor.ProcessMessage(message);

		var expectedInterpretation =
						"<style>" +
							"table, th, td { " +
								"border: 1px solid black; " +
								"border-collapse: collapse; " +
							"} " +
							"th, td { " +
								"padding: 5px; " +
								"text-align: left; " +
							"}" +
						"</style>" +
						"<h2>CC531C - Extended deadline for submitting a supplementary declaration</h2><hr />" +
						"<table>" +
							"<tbody>" +
							   $"<tr><th>Customs office of export sending the message</th><td>{testCustomsOfficeOfExportReferenceNumber}</td></tr>" +
							   $"<tr><th>MRN</th><td>{testMrn}</td></tr>" +
							   $"<tr><th>Lodgement of supplementary declaration start date</th><td>23/12/2024</td></tr>" +
							   $"<tr><th>Lodgement of supplementary declaration expiry date</th><td>23/12/2024</td></tr>" +
							   $"<tr><th>Timer expiry information</th><td>{testTimerExpiryInformation}</td></tr>" +
							"</tbody>" +
						"</table>";
		
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", expectedMessageStatus, message.EM_Status);
			AssertEquals("EM_MessageInterpretation", expectedInterpretation, message.EM_MessageInterpretation );
		});
	}

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC531C - Extended deadline for submitting a supplementary declaration" + " - " + mrn + " Response";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}
}
