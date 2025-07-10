using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
//using Enterprise.Customs.TR.Business.MessagingProcess.Testing;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.MessagingProcess.Testing
{
	sealed class TRManifestCustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<NotSupportedException>("Invalid message type", () => { var crash = TRManifestCustomsMessagingProvider.New(header, "CRASH"); });
				AssertNoExceptionThrown("Valid message type", () => { var provider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO); });
			});
		}

		public void TestConstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var provider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO);

			var messengers = provider.GetMessengers();
			AssertEquals(1, messengers.Count);
			AssertType<TRCustomsMessenger>(messengers.FirstOrDefault());
		}

		public void TestITRCustomsMessagingProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var trProvider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO, null) as ITRCustomsMessagingProvider;

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
				var provider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO, null);

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
			var provider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO);

			header.AMA_MessageStatus = TRMessageStatusCodeList.Codes.Awaiting;

			var notifications = string.Join("\n", provider.RunPreSendValidation(new ActionResult(true)).Select(x => $"{x.MessageIncludingPrefix}"));

			AssertContains("Status warning", "Warning: The status is wait for response message, if you send again, the message will be rejected.", notifications);
		}

		public void TestISupportPreSendValidation_Commmon()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var provider = TRManifestCustomsMessagingProvider.New(header, TRMessageTypes.Codes.TRO, null) as ISupportPreSendValidation;

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
