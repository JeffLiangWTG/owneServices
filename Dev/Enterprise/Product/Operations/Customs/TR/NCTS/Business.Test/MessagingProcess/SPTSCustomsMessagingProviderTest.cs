using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.MessagingProcess.Testing
{
	sealed class SPTSCustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = Factory.New<SPTSHeader>();
			var provider = SPTSCustomsMessagingProvider.New(header, string.Empty, null);

			AssertType<SPTSCustomsMessagingProvider>(provider);
		}

		public void TestGetMessengers()
		{
			var header = Factory.New<SPTSHeader>();

			var provider = SPTSCustomsMessagingProvider.New(header, string.Empty, null);
			var trMessenger = provider.GetMessengers().Single();
			AssertType<TRCustomsMessenger>(trMessenger);

			var generator = trMessenger.MessageGenerator;
			AssertType<SPTSMessageGenerator>(generator);
		}

		public void TestITRCustomsMessagingProvider()
		{
			var header = Factory.New<SPTSHeader>();
			var trProvider = SPTSCustomsMessagingProvider.New(header, string.Empty, null) as ITRCustomsMessagingProvider;

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
				var header = Factory.New<SPTSHeader>();
				var provider = SPTSCustomsMessagingProvider.New(header, string.Empty, null);

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
			var header = Factory.New<SPTSHeader>();
			var provider = SPTSCustomsMessagingProvider.New(header, string.Empty, null);

			CombineAssertions(() =>
			{
				var notifications = string.Join("\n", provider.RunPreSendValidation(new ActionResult(true)).Select(x => $"{x.MessageIncludingPrefix}"));
				AssertNotContains("No notifications when registration empty", "Error: You are sending a SPTS Declaration which has a Registration No", notifications);

				header.RegistrationNumber = "ABC213";
				notifications = string.Join("\n", provider.RunPreSendValidation(new ActionResult(true)).Select(x => $"{x.MessageIncludingPrefix}"));
				AssertContains("Content", "Error: You are sending a SPTS Declaration which has a Registration No\r\nPlease first use Amend SPTS Declaration menu item to prepare the declaration for resending", notifications);
			});
		}

		public void TestISupportPreSendValidation_Common()
		{
			var header = Factory.New<SPTSHeader>();
			var provider = SPTSCustomsMessagingProvider.New(header, string.Empty, null) as ISupportPreSendValidation;

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
