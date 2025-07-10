using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.TestHelpers.Xml;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestsSubclassesOf(typeof(ExitControlMessageSender))]
abstract class ExitControlMessageSenderTest<TMessageSender> : TestCaseWithFactory
	where TMessageSender : ExitControlMessageSender
{
	public void TestSend() => CombineAssertions(() =>
	{
		var testData = new ExitControlTestData(Factory);
		var exitReport = testData.ExitReport;
		var messageSender = CreateSender(exitReport);

		if (!SendMessageAndAssertResult(messageSender, exitReport, out var message))
		{
			return;
		}

		AssertEquals("EM_SendWithMessageErrors", false, message.EM_SendWithMessageErrors);
		AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.PLCustomsExitControl, message.EM_ApplicationCode);
		AssertEquals("EM_ApplicationReference", messageSender.ApplicationReference , message.EM_ApplicationReference);
		AssertEquals("EM_MessageType", EdiMessageMessageType.ExitControl, message.EM_MessageType);
		AssertEquals("EM_MessageSubType", messageSender.MessageSubType, message.EM_MessageSubType);
		AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
		AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
		AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, message.EM_GE);
		AssertEquals("EM_GP", GlbStaff.CurrentUser.GetPLWrapper()?.PLBPassword.PK, message.EM_GP);

		AssertEquals("EM_LinkedObject", exitReport, message.EM_LinkedObject);

		AssertEquals("LastSentMessage", message, messageSender.LastSentMessage);

		var expectedMessageXml = TestMessageXml;
		var actualMessageXml = message.EM_MessageText;
		XmlComparison.CompareAndAssertXml(expectedMessageXml, actualMessageXml);
	});

	protected bool SendMessageAndAssertResult(TMessageSender messageSender, CusExitReport exitReport, out EDIMessage message, string prefix = "")
	{
		var messagesBeforeSend = exitReport.Messages.Count;
		messageSender.Send();
		AssertEquals($"{prefix} 1 message was send", 1, exitReport.Messages.Count - messagesBeforeSend);
		if (exitReport.Messages.Count - messagesBeforeSend != 1)
		{
			message = null;
			return false;
		}

		message = (EDIMessage)exitReport.Messages.Last();
		return true;
	}

	protected virtual string TestMessageXml => "<Something>123</Something>";

	protected virtual IXmlMessageBuilder GetXmlMessageBuilder()
		=> Mock.Of<IXmlMessageBuilder>(b
			=> b.GenerateXmlMessage() == Mock.Of<IXmlMessage>(m
				=> m.GetSerializedStream() == new MemoryStream(Encoding.UTF8.GetBytes(TestMessageXml))));

	protected virtual TMessageSender CreateSender(CusExitReport exitReport)
	{
		base.SetUp();

		var messageSendingObject = new ExitControlMessageSendingObject(exitReport);
		var messageSenderMock = new Mock<TMessageSender>(messageSendingObject) { CallBase = true };
		messageSenderMock.Protected()
					.Setup<IXmlMessageBuilder>("GetXmlMessageBuilder")
					.Returns(GetXmlMessageBuilder);
		return messageSenderMock.Object;
	}
}
