using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Manifest.Business.MessagingProcess.Testing
{
	class TRManifestCustomsMessengerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<NotSupportedException>("Invalid message type", () => { var crash = TRManifestCustomsMessenger.New(header, "CRASH"); });
				var messenger = TRManifestCustomsMessenger.New(header, TRMessageTypes.Codes.TRO);
				AssertType<TRCustomsMessenger>(messenger);
			});
		}

		public void TestConstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var trMessenger = TRManifestCustomsMessenger.New(header, TRMessageTypes.Codes.TRO);
			var messenger = trMessenger as ICustomsMessenger;
			AssertSame(header, messenger.Owner);
		}

		public void TestCreateMessageGenerator()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			AssertMsgTypeGeneratorAndProvider(header, TRMessageTypes.Codes.TRO, typeof(TRManifestMessageGenerator), typeof(ManifestMessageProvider));
		}

		void AssertMsgTypeGeneratorAndProvider(AsycudaManifestHeader header, ZString msgType, Type senderType, Type providerType)
		{
			var trMessenger = TRManifestCustomsMessenger.New(header, msgType);
			var generator = ((ICustomsMessenger)trMessenger).MessageGenerator;
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
