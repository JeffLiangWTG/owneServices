using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.ETrade.Business.MessagingProcess.Testing
{
	class ETradeCustomsMessengerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<NotSupportedException>("Invalid message type", () => { var crash = ETradeCustomsMessenger.New(header, "CRASH"); });
				var messenger = ETradeCustomsMessenger.New(header, TRMessageTypes.Codes.TRE);
				AssertType<TRCustomsMessenger>(messenger);
			});
		}

		public void TestConstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var trMessenger = ETradeCustomsMessenger.New(header, TRMessageTypes.Codes.TRE);
			var messenger = trMessenger as ICustomsMessenger;
			AssertSame(header, messenger.Owner);
		}

		public void TestCreateMessageSender()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRE, typeof(ETradeTemporaryRegistrationMessageGenerator), typeof(ETradeTemporaryRegistrationMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRQ, typeof(ETradeQueryForRegNoMessageGenerator), typeof(ETradeQueryForRegNoMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRS, typeof(ETradeSendForRegistrationNoMessageGenerator), typeof(ETradeSendForRegistrationNoMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRI, typeof(ETradeQueryForInspectionClerkMessageGenerator), typeof(ETradeQueryForInspectionClerkMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRL, typeof(ETradeQueryForInspectionLineMessageGenerator), typeof(ETradeQueryForInspectionLineMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRB, typeof(ETradeQueryRemainingBillsforImpDecMessageGenerator), typeof(ETradeQueryRemainingBillsforImpDecMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TRD, typeof(ETradeDischargeListMessageGenerator), typeof(ETradeDischargeListMessageProvider));
			AssertMsgTypeSenderAndProvider(header, TRMessageTypes.Codes.TCD, typeof(ETradeComplementaryDecMessageGenerator), typeof(ETradeComplementaryDecMessageProvider));
		}

		void AssertMsgTypeSenderAndProvider(AsycudaManifestHeader header, ZString msgType, Type senderType, Type providerType)
		{
			var messenger = ETradeCustomsMessenger.New(header, msgType) as ICustomsMessenger;
			var generator = messenger.MessageGenerator;
			CombineAssertions($"Message Type: {msgType}", () =>
			{
				AssertEquals("Sender Type", senderType, generator.GetType());

				var senderField = generator.GetType().GetField("Sender", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				AssertNotNull("Sender Field", senderField);
				AssertEquals("Provider Type", providerType, senderField.GetValue(generator).GetType());
			});
		}
	}
}
