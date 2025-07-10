using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC521CMessageProcessor))]
sealed class CC521CMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<CC521CMessageProcessor, ICC521C>
{
	protected override string ExpectedMessageFriendlyName => ExitControlMessageCodes.Descriptions.CC521;

	protected override Type ExpectedMessageInterpreterType => typeof(CC521CMessageInterpreter);

	protected override bool ExpectedIsFailureNotification => false;

	public void TestProcessMessage()
	{
		dataProviderMock.Setup(x => x.ExportOperation).Returns(new Mock<ICC521CExportOperation>().Object);

		var exitReport = Factory.New<CusExitReport>();
		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = AutoCusExitReport.Schema.TableName;
		message.EM_LinkUniqueID = exitReport.PK;
		message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals(message: nameof(CusExitReport.CER_Status), expected: "DRJ", actual: exitReport.CER_Status);
			AssertEquals(message: nameof(CusExitReport.CER_MessageStatus), expected: "RCV", exitReport.CER_MessageStatus);
			AssertEquals(message: nameof(EDIMessage.EM_Status), expected: EDIMessageStatusList.Codes.ProcessedOK, actual: message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string mrn = "MRN_TEST";
		const string expectedSubject = $"CC521C - Diversion rejection notification. - {mrn} Response";

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(new Mock<ICC521CExportOperation>().Object);

		ProcessMessageAndTestEmail(expectedSubject, "MessageNo", mrn);
	}
}
