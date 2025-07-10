using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Customs.NZ.Registry;

	public abstract class SendICRBaseTest : SendTSWBaseTest
	{ }

	class SendICRTest : SendICRBaseTest
	{
		public void TestMessageSubTypeOriginal()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			icrSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageSubTypeCancellation()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Cancel);
			icrSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageSubTypeReplace()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Replace);
			icrSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Replacement, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageType()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			icrSender.SendMessage();
			var messages = dummyObject.Messages;
			AssertEquals(1, messages.Count);
			AssertEquals(MessageTypeList.Codes.ICR, messages[0].EM_MessageType);
		}

		public void TestGetIsMessageInTestMode()
		{
			using (NZCustomsDataRegistry.Instance.ImportEciTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTestMode(true);
			}

			using (NZCustomsDataRegistry.Instance.ImportEciTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTestMode(false);
			}
		}

		void AssertTestMode(bool isTestMode)
		{
			dummyObject = Factory.New<DummyBusinessObjectWithMessages>();
			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			icrSender.SendMessage();
			AssertEquals(isTestMode, dummyObject.Messages[0].EM_IsTestMessage);
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new TestSendICR(dummyObject, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			dummyObject = Factory.New<DummyBusinessObjectWithMessages>();
		}

		DummyBusinessObjectWithMessages dummyObject;
	}

	public class TestSendICR : SendICR
	{
		public TestSendICR(DummyBusinessObjectWithMessages hostEntity, TSWTransactionTypes transactionType) : base(hostEntity, null, transactionType)
		{
			this.hostEntity = hostEntity;
		}

		readonly DummyBusinessObjectWithMessages hostEntity;

		public override ZString DeclarantPinEncrypted => ZString.Empty;

		public override ZBool DeclarantPinRequired => false;

		public override ZString ApplicationReference => ZString.Empty;

		protected override void AddMessageToMessages(TSWMessage message)
		{
			hostEntity.Messages.Add(message);
		}

		public override ZString GetMessageText() => ZString.Empty;

		protected override void CheckErrorsBeforeGeneratingMessageCore() { }

		protected override void SetStatusOnSuccess(StatusTransactionScope scope) { }

		protected override ICRMessageBuilder GetICRMessageBuilder() => null;
	}
}
