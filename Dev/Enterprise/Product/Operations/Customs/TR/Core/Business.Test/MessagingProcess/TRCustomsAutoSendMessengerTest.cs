using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.TR.Business.MessagingProcess.Testing
{
	class TRCustomsAutoSendMessengerTest : TestCaseWithFactory
	{
		public void TestSendMessage_NoAction()
		{
			var owner = SetupAndSend(null);

			AssertEquals("Message created and linked", 1, owner.Messages.Count);
		}

		public void TestSendMessage_WithAction()
		{
			var notificationFromAction = "No change yet";
			bool? successFromAction = null;
			var owner = SetupAndSend((result) =>
			{
				notificationFromAction = result.Notifications.NotificationsAsString();
				successFromAction = result.Success;
			});

			AssertEquals("Message created and lined", 1, owner.Messages.Count);
			AssertEquals("Value changed by action", "Message sent successfully.\r\n", notificationFromAction);
			AssertEquals("Success", true, successFromAction);
		}

		public void TestIMessageAttacheeMessageStatusUpdate()
		{
			var owner = SetupAndSend(null) as IMessageAttachee;

			AssertNotNull(owner);
			AssertEquals(TRMessageStatusCodeList.Codes.Awaiting, owner.MessageStatus);
		}

		public void TestSignMessages()
		{
			var (owner, sender, generator) = SetupOwnerSenderAmdGenerator();
			var messenger = new TRCustomsAutoSendMessenger(sender, generator) as ICustomsMessenger;

			AssertEquals("No signing", string.Empty, messenger.SignMessages(new[] { generator.GenerateMessage() }, new ActionResult()));
		}

		public void TestShouldCreateMessage()
		{
			var (owner, sender, generator) = SetupOwnerSenderAmdGenerator();
			var messenger = new TRCustomsAutoSendMessenger(sender, generator) as ICustomsMessenger;

			AssertEquals("Always create", true, messenger.ShouldCreateMessage(new ActionResult()));
		}

		public void TestOwner()
		{
			var (owner, sender, generator) = SetupOwnerSenderAmdGenerator();
			var messenger = new TRCustomsAutoSendMessenger(sender, generator) as ICustomsMessenger;

			AssertSame(owner.MessageOwner, messenger.Owner.MessageOwner);
		}

		public void TestGenerator()
		{
			var (owner, sender, generator) = SetupOwnerSenderAmdGenerator();
			var messenger = new TRCustomsAutoSendMessenger(sender, generator) as ICustomsMessenger;

			AssertSame(generator, messenger.MessageGenerator);
		}

		IEDIMessageCollectionOwner SetupAndSend(Action<ActionResult> actionAfterSend)
		{
			var (child, sender, generator) = SetupOwnerSenderAmdGenerator();

			TRCustomsAutoSendProviderFactory.SendMessage<DummyBizObjWrapper>(child, (bo) => generator, actionAfterSend);

			return child;
		}

		(IEDIMessageCollectionOwner owner, IMessageSender sender, ITRCustomsMessageGenerator messageGenerator) SetupOwnerSenderAmdGenerator()
		{
			var child = Factory.New<DummyBizObjWithMessagesAndIMessageAttachee>();
			var mockMessageGenerator = new Mock<ITRCustomsMessageGenerator>();
			var msg = Factory.New<DummyEDIMessage_TRCustomsAutoSendMessengerTest>();
			msg.GetMessageReferenceNumberToReturn = "1";

			var sender = new Mock<IMessageSender>();
			sender.Setup(m => m.Parent).Returns(child);
			sender.Setup(m => m.Messages).Returns(child.Messages);

			mockMessageGenerator.Setup(m => m.GenerateMessage()).Returns(msg);

			return (child, sender.Object, mockMessageGenerator.Object);
		}
	}

	public class DummyEDIMessage_TRCustomsAutoSendMessengerTest : EDIMessage
	{
		public DummyEDIMessage_TRCustomsAutoSendMessengerTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string GetMessageReferenceNumberToReturn { get; set; }

		protected override string GetMessageReferenceNumber()
		{
			return GetMessageReferenceNumberToReturn;
		}
	}

	public class DummyBizObjWithMessagesAndIMessageAttachee : DummyBizObjWithMessages, IMessageAttachee
	{
		public DummyBizObjWithMessagesAndIMessageAttachee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString MessageStatus { get; set; }
		public ZString CustomsStatus { get; set; }

		public ZString JobReference => "Job123";

		public ZGuid GlobalBranchPK => Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK;

		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
	}

	class DummyBizObjWrapper : IMessageSender
	{
		public DummyBizObjWrapper(DummyBizObjWithMessagesAndIMessageAttachee bizObj)
		{
			this.bizObj = bizObj;
		}
		readonly DummyBizObjWithMessagesAndIMessageAttachee bizObj;
		BusinessObject IMessageSender.Parent => bizObj;

		IBusinessObjectCollection IMessageSender.Messages => bizObj.Messages;

		ZString IMessageSender.JobReference => "Job911";
	}
}
