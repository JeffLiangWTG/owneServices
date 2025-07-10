using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestApplicationReference()
		{
			var sender = GetSender("b5d51907-d1bf-4852-91c6-5c33e535a167");
			AssertEquals("ApplicationReference", "b5d51907-d1bf-4852-91c6-5c33e535a167", sender.ApplicationReferenceExposed);
		}

		public void TestWhoSendThisMessage()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			var originalMessage = Factory.New<TRBaseMessage>();
			originalMessage.EM_SystemCreateUser = "YE";
			var sender = GetSender(originalMessage);
			AssertEquals("MessageOwner", "YE", sender.WhoSendThisMessageExposed.GS_Code);
		}

		public void TestMessageSubType()
		{
			var originalMessage = Factory.New<TRBaseMessage>();
			originalMessage.EM_MessageSubType = "XXX";
			var sender = GetSender(originalMessage);
			AssertEquals("MessageSubType", "XXX", sender.MessageSubTypeExposed);
		}

		TRAutoReceiveResponseMessageSenderForTest GetSender(TRBaseMessage originalMessage = null)
		{
			return GetSender(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		TRAutoReceiveResponseMessageSenderForTest GetSender(ZString guid, TRBaseMessage originalMessage = null)
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var provider = new ETradeAutoReceiveResponseMessageProvider(header);
			return new TRAutoReceiveResponseMessageSenderForTest(provider, guid, originalMessage);
		}
	}

	class TRAutoReceiveResponseMessageSenderForTest : TRAutoReceiveResponseMessageGenerator<TRBaseMessage>
	{
		public TRAutoReceiveResponseMessageSenderForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
		{
		}

		public ZString ApplicationReferenceExposed => base.ApplicationReference;

		public GlbStaff WhoSendThisMessageExposed => base.WhoSendThisMessage;

		public ZString MessageSubTypeExposed => base.MessageSubType;

		public override ZString MessageType => throw new NotImplementedException();

		protected override ZString MessageText => throw new NotImplementedException();
	}
}
