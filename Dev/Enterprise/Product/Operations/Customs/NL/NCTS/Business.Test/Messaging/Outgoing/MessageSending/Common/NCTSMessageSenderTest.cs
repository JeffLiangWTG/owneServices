using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

public abstract class NCTSMessageSenderTest<TMessageSender, TProvider> : TestCaseWithFactory
	where TMessageSender : MessageSender<TProvider>
	where TProvider : class, IMessageHeader
{
	protected virtual void AssertMessage(NLEDIMessage message)
	{
		AssertMessageStatusAndEdiMessageDetails(message);
		AssertMessageContent(message);
	}

	public void TestPostSendProcess()
	{
		messageSender.Send();
		NLEDIMessage message;

		if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
		{
			message = (NLEDIMessage)((NctsHeader)messageSender.MessageObject).MovementHeader.Messages.First();
		}
		else
		{
			message = (NLEDIMessage)((NctsHeader)messageSender.MessageObject).Messages.First();
		}

		var expectedMessageInterpretation = new NctsEdiMessagePrettier(message).MakeOutboundPrettyForInterpretation(nctsHeader);
		var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
		CombineAssertions(() =>
		{
			AssertEquals("Customs status", ExpectedCustomsStatus, movementHeader.BM_CustomsStatus);
			AssertEquals("Phase", ExpectedPhaseStatus, movementHeader.BM_Phase);
			AssertEquals("Message status", ExpectedMessageStatus, nctsHeader.EffectiveMessageStatus);
			AssertEquals("Message interpretation", expectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}

	public void TestSendMessage()
	{
		messageSender.Send();

		AssertEquals("Test-message is not checked in SendingObject (live)", false, messageSender.IsTestMessage);
		AssertMessageObject();
	}

	public void TestSendMessage_TestDeclaration()
	{
		SetUp_TestDeclaration();
		messageSender.Send();

		AssertEquals("Test-message is checked in SendingObject (test)", true, messageSender.IsTestMessage);
		AssertMessageObject();
	}

	protected string MessageDirectory => @"Enterprise.Customs.NL.NCTS.Business.Testing.Messaging.Outgoing.TestFiles.";

	protected Assembly XmlContentAssembly => Assembly.GetExecutingAssembly();

	protected ZString MessageType => EDIInterchange.ApplicationCodes.EuNcts;

	protected virtual ZString ParentTableName => CusInBondHeader.Schema.TableName;

	protected abstract string MovementType { get; }

	protected virtual ZString ExpectedCustomsStatus => ZString.Empty;

	protected abstract ZString ExpectedPhaseStatus { get; }

	protected ZString ExpectedMessageStatus => LogicalStatusList.Codes.Sent;

	protected abstract ZString EntryType { get; }

	protected virtual IEnumerable<PropertyInfo> IgnorePropertiesFromTest => Enumerable.Empty<PropertyInfo>();

	protected ZGuid LinkedObjectId
	{
		get
		{
			ZGuid id;

			if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
			{
				id = ((NctsHeader)messageSender.MessageObject).MovementHeader.PK;
			}
			else
			{
				id = ((NctsHeader)messageSender.MessageObject).PK;
			}

			return id;
		}
	}

	protected void SetUp_TestDeclaration() => CommonSetup(true);

	void AssertMessageContent(NLEDIMessage message)
	{
		AssertNotNullOrEmpty(message.EM_MessageText);
	}

	void AssertMessageStatusAndEdiMessageDetails(NLEDIMessage message)
	{
		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.NLCustoms, message.EM_ApplicationCode);
			AssertNullOrEmpty("EM_MessageOwner", message.EM_MessageOwner);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageSubType", messageSender.MessageSubType, message.EM_MessageSubType);
			AssertEquals("EM_MessageType", MessageType, message.EM_MessageType);
			AssertNullOrEmpty("EM_ApplicationReference", message.EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("EM_GB", EnvProxy.Instance.CurrentBranch.PK, message.EM_GB);
			AssertEquals("EM_GE", Env.CurrentDepartment.PK, message.EM_GE);
			AssertNotNullOrEmpty("EM_MessageText", message.EM_MessageText);
			Assert("EM_IsActive", message.EM_IsActive);
			AssertEquals("EM_LinkTable", ParentTableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", LinkedObjectId, message.EM_LinkUniqueID);
			AssertEquals("EM_IsTestMessage", messageSender.IsTestMessage, message.EM_IsTestMessage);
		});
	}

	void AssertMessageObject()
	{
		if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
		{
			AssertMessage((NLEDIMessage)((NctsHeader)messageSender.MessageObject).MovementHeader.Messages.First());
		}
		else
		{
			AssertMessage((NLEDIMessage)((NctsHeader)messageSender.MessageObject).Messages.First());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		CommonSetup(false);
	}

	void CommonSetup(bool isTestDeclaration)
	{
		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
		action = new MessageSendingAction(movementHeader) { MessageType = EntryType, IsTestDeclaration = isTestDeclaration };

		mockProvider = new Mock<TProvider> { CallBase = true };
		var mockMessageSender = new Mock<TMessageSender>(action) { CallBase = true };
		mockMessageSender.Protected().Setup<TProvider>("GetDataProvider", nctsHeader)
			.Returns(mockProvider.Object);
		messageSender = mockMessageSender.Object;
	}

	protected MessageSendingAction action;
	protected NctsHeader nctsHeader;
	protected TMessageSender messageSender;
	protected Mock<TProvider> mockProvider;
}
