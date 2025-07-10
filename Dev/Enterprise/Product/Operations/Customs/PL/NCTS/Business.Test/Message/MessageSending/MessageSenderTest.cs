using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(MessageSender))]
public abstract class MessageSenderAbstractTest<T> : TestCaseWithFactory
	where T : MessageSender
{
	public void TestMessageSubTypeLength()
	{
		(_, var messageSender) = CreateMessageSender();
		var messageSybType = messageSender.GetType()
			.GetProperty("MessageSubType", BindingFlags.NonPublic | BindingFlags.Instance)
			.GetValue(messageSender).ToString();
		AssertEquals("The lenght of MessageSubType should be equal to 3 characters.", 3, messageSybType.Length);
	}

	protected (NctsHeader nctsHeader, T messageSender) CreateMessageSender()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(HeaderType);
		var messageSendingObject = new MessageSendingObject(nctsHeader);
		var messageSender = (T)Activator.CreateInstance(typeof(T), messageSendingObject);
		return (nctsHeader, messageSender);
	}

	protected abstract ZString HeaderType { get; }
}

sealed class MessageSenderBaseOnlyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new MessageSenderForTest(null));
			AssertNoExceptionThrown("MessageSendingObject is not null", () => new MessageSenderForTest(messageSendingObject));
		});
	}

	public void TestSend()
	{
		messageSender.Send();
		CombineAssertions(() =>
		{
			AssertEquals("Message was send", 1, nctsHeader.MovementHeader.Messages.Count);

			var message = (EDIMessage)nctsHeader.MovementHeader.Messages.FirstOrDefault();
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.PLCustomsNCTS, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageSenderForTest.TestMessageType, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSenderForTest.TestMessageSubType, message.EM_MessageSubType);
			AssertEquals("EM_ApplicationReference", string.Empty, message.EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			AssertEquals("EM_LinkedObject", nctsHeader.MovementHeader, message.EM_LinkedObject);
			AssertEquals("EM_MessageText", MessageSenderForTest.TestMessageContent, message.EM_MessageText);
			AssertEquals("EM_IsActive", false, message.EM_IsActive);
			AssertEquals("EM_IsTestMessage", true, message.EM_IsTestMessage);

			var expectedUserCode = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("EM_SystemLastEditUser", expectedUserCode, message.EM_SystemLastEditUser);
			AssertEquals("EM_SystemCreateUser", expectedUserCode, message.EM_SystemCreateUser);

			var expectedBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals("EM_GB", expectedBranch, message.EM_GB);

			var expectedDepartment = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("EM_GE", expectedDepartment, message.EM_GE);

			var expectedPassword = GlbStaff.CurrentUser.GetPLWrapper()?.PLBPassword.PK;
			AssertEquals("EM_GP", expectedPassword, message.EM_GP);

			AssertNotEquals("Poland is using EM_MessageInterpretation", string.Empty, message.EM_MessageInterpretation);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);

		messageSender = new MessageSenderForTest(messageSendingObject);
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
	MessageSenderForTest messageSender;

	sealed class MessageSenderForTest : MessageSender
	{
		public const string TestMessageContent = "<Something>123</Something>";
		public const string TestMessageType = "MSG";
		public const string TestMessageSubType = "TST";

		public MessageSenderForTest(MessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override ZString MessageType => TestMessageType;

		protected override ZString MessageSubType => TestMessageSubType;

		protected override ZString PhaseCode => ZString.Empty;

		protected override IXmlMessageBuilder GetMessageBuilder()
		{
			var messageBuilderMock = new Mock<IXmlMessageBuilder>();
			var xmlMessage = Mock.Of<IXmlMessage>(m => m.GetSerializedStream() == new MemoryStream(Encoding.UTF8.GetBytes(TestMessageContent)));
			messageBuilderMock.Setup(x => x.GenerateXmlMessage()).Returns(xmlMessage);
			return messageBuilderMock.Object;
		}
	}
}
