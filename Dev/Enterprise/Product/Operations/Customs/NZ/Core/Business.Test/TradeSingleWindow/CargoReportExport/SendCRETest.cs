using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;

	public abstract class SendCREBaseTest : SendTSWBaseTest
	{
	}

	public class SendCRETest : SendCREBaseTest
	{
		public void TestMessageSubTypeOriginal()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Original);
			creSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageSubTypeCancellation()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Cancel);
			creSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageSubTypeReplace()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Replace);
			creSender.SendMessage();
			AssertEquals(Business.Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Replacement, dummyObject.Messages[0].EM_MessageSubType);
		}

		public void TestMessageType()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Original);
			creSender.SendMessage();
			var messages = dummyObject.Messages;
			AssertEquals(1, messages.Count);
			var message = messages[0];
			AssertEquals(MessageTypeList.Codes.CRE, message.EM_MessageType);
		}

		public void TestGetIsMessageInTestMode()
		{
			using (NZCustomsDataRegistry.Instance.ExportEciTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTestMode(true);
			}

			using (NZCustomsDataRegistry.Instance.ExportEciTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTestMode(false);
			}
		}

		void AssertTestMode(bool isTestMode)
		{
			dummyObject = Factory.New<DummyBusinessObjectWithMessages>();
			var creSender = GetNewSender(TSWTransactionTypes.Original);
			creSender.SendMessage();
			AssertEquals(isTestMode, dummyObject.Messages[0].EM_IsTestMessage);
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new TestSendCRE(dummyObject, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L");
			dummyObject = Factory.New<DummyBusinessObjectWithMessages>();
		}

		DummyBusinessObjectWithMessages dummyObject;
	}

	public class TestSendCRE : SendCRE
	{
		public TestSendCRE(DummyBusinessObjectWithMessages hostEntity, TSWTransactionTypes transactionType) : base(hostEntity, null, transactionType)
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

		protected override CREMessageBuilder GetCREMessageBuilder() => null;

		protected override void SetStatusOnSuccess(StatusTransactionScope scope) { }
	}
}
