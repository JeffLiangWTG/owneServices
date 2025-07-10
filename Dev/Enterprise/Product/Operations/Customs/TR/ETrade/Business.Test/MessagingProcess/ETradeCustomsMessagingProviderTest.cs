using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.MessagingProcess.Testing
{
	class ETradeCustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestPreSendValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MessageStatus = TRMessageStatusCodeList.Codes.Awaiting;

			var eTradeProvider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null);

			var notifications = string.Join("\n", eTradeProvider.RunPreSendValidation(new ActionResult(true)).Select(x => $"{x.MessageIncludingPrefix}"));

			AssertContains("Status warning", "Warning: The status is wait for response message, if you send again, the message will be rejected.", notifications);
		}

		public void TestNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var eTradeProvider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null);

			AssertType<ETradeCustomsMessagingProvider>(eTradeProvider);
		}

		public void TestGetMessengers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var eTradeProvider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null);

			var messenger = eTradeProvider.GetMessengers().Single();
			AssertType<TRCustomsMessenger>(messenger);
			var generator = messenger.MessageGenerator;
			AssertType<ETradeTemporaryRegistrationMessageGenerator>(generator);
		}

		public void TestGetMessengers_Unsupported()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			AssertExceptionThrown<NotSupportedException>("Invalid message type", () => ETradeCustomsMessagingProvider.New(header, "Crash", null));
		}

		public void TestITRCustomsMessagingProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var trProvider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null) as ITRCustomsMessagingProvider;

			AssertNotNull("Is ITRCustomsMessagingProvider", trProvider);

			CombineAssertions(() =>
			{
				AssertNotNull("TRBPassword", trProvider.TRBPassword);
				AssertNotNull("Messenger", trProvider.TRMessengers.FirstOrDefault());
			});
		}

		public void TestICustomsMessagingProvider()
		{
			using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var provider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null);

				CombineAssertions(() =>
				{
					AssertEquals("IsIntestMode", true, provider.IsInTestMode);
					AssertEquals("Enable Test Mode validation", false, provider.EnableTestModeValidation);
					AssertNotNull("Messenger created", provider.GetMessengers());
				});
			}
		}

		public void TestISupportPreSendValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var provider = ETradeCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRE, null) as ISupportPreSendValidation;

			var results = provider.RunPreSendValidation(new ActionResult(true)).ToList();

			AssertEquals("3 Errors", 3, results.Count);
			var errors = string.Join("\n", results.Select(x => x.Message));

			CombineAssertions(() =>
			{
				AssertContains("Staff error", "Your staff profile requires an email address", errors);
				AssertContains("Customs Credentials", "Your customs credentials are marked as invalid", errors);
				AssertContains("Certificate", "A valid certificate serial number is required", errors);
			});
		}
	}
}
