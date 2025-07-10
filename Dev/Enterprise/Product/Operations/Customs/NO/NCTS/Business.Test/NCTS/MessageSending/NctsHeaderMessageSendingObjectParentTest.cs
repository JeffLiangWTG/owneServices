using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NO.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
sealed class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderMessageSendingObjectParent(null));
	}

	public void TestSendingObjectCollection()
	{
		var nctsHeader = CreateNewNctsHeader();
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObjectCollection = sendingObjectParent.SendingObjectsCollection;

		AssertType<NctsHeaderMessageSendingObjectCollection>(sendingObjectCollection);
		AssertEquals("Message Sending Object Collection Count", 1, sendingObjectCollection.Count);
		AssertType<NctsHeaderMessageSendingObject>(sendingObjectCollection[0]);
	}

	public void TestSendAndSaveMessage_ForArrivalMessageC007C()
	{
		var nctsHeader = CreateNewNctsHeader();
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = sendingObjectParent.SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification;

		var isSuccess = sendingObjectParent.SendAndSaveMessages();
		CombineAssertions(() =>
		{
			AssertEquals("Operation Successful", true, isSuccess);

			var arrivalMessages = nctsHeader.Messages;
			arrivalMessages.Reload(reLoadExistingRows: false);
			AssertEquals("Messages Count", 1, arrivalMessages.Count);
#if NETFRAMEWORK
			AssertContains("Message Text", "<q1:CC007C xmlns:q1=\"http://ncts.dgtaxud.ec\">", arrivalMessages[0].EM_MessageText);
#else
			AssertContains("Message Text", "<CC007C xmlns=\"http://ncts.dgtaxud.ec\">", arrivalMessages[0].EM_MessageText);
#endif

			AssertEquals("Count of Messages in Messages Collection", 1, nctsHeader.Messages.Count);
			AssertEquals("EDIMessageType", "007", nctsHeader.Messages[0].EM_MessageType);
		});
	}

	public void TestSendAndSaveMessage_ForDepartureMessage()
	{
		var nctsHeader = CreateNewNctsHeader();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = sendingObjectParent.SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = NctsDepartureMessageTypeCodeList.Codes.HelpMeDecide;
		AssertExceptionThrown<NotSupportedException>(() => sendingObjectParent.SendAndSaveMessages());
	}

	public void TestEffectiveMessageStatus_OnSendAndSaveMessage_ForArrivalNotification()
	{
		var nctsHeader = CreateNewNctsHeader();
		var messageBuilderMock = new Mock<IOutboundMessageBuilder>();
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParentForTest(nctsHeader, messageBuilderMock.Object);
		var sendingObject = sendingObjectParent.SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>().First();

		CombineAssertions(() =>
		{
			sendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification;
			messageBuilderMock.Setup(m => m.Create(It.IsAny<IMessageInformationProvider>()))
				.Returns(CreateEDIMessageForTest(NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification));
			sendingObjectParent.SendAndSaveMessages();
			AssertEquals("When MessageType is Arrival Notification (007)", NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);

			sendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks;
			messageBuilderMock.Setup(m => m.Create(It.IsAny<IMessageInformationProvider>()))
				.Returns(CreateEDIMessageForTest(NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks));
			sendingObjectParent.SendAndSaveMessages();
			AssertEquals("When MessageType is Unloading Remarks (044)", NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent, nctsHeader.EffectiveMessageStatus);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(CreateNewNctsHeader());

	OutboundEDIMessage CreateEDIMessageForTest(string messageType)
	{
		var message = Factory.NewWithValidTestData<OutboundEDIMessage>();
		message.EM_MessageType = messageType;
		message.MessageNumberStrategy = Mock.Of<IMessageNumberStrategy>(s => s.GetMessageReferenceNumber() == "1");
		return message;
	}

	NctsHeader CreateNewNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_ArrivalDate = new ZDateTime(2024, 01, 02);
		return nctsHeader;
	}

	sealed class NctsHeaderMessageSendingObjectParentForTest : NctsHeaderMessageSendingObjectParent
	{
		public NctsHeaderMessageSendingObjectParentForTest(NctsHeader nctsHeader, IOutboundMessageBuilder messageBuilder) : base(nctsHeader)
		{
			this.messageBuilder = messageBuilder;
		}
		readonly IOutboundMessageBuilder messageBuilder;

		protected override IOutboundMessageBuilder GetOutboundMessageBuilder() => messageBuilder;
	}
}
