using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.MessagingProcess.Testing
{
	sealed class Phase5NctsCustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = Factory.New<NctsHeader>();

			var provider = Phase5NctsCustomsTR5MessagingProvider.New(header, string.Empty, null);
			AssertType<Phase5NctsCustomsTR5MessagingProvider>(provider);
		}

		public void TestGetMessengers()
		{
			var header = Factory.New<NctsHeader>();

			var provider = Phase5NctsCustomsTR5MessagingProvider.New(header, string.Empty, null);
			var trMessenger = provider.GetMessengers().Single();
			AssertType<TRCustomsMessenger>(trMessenger);

			var generator = trMessenger.MessageGenerator;
			AssertType<TR5MessageGenerator>(generator);
		}

		public void TestITRCustomsMessagingProvider()
		{
			var header = Factory.New<NctsHeader>();
			var trProvider = Phase5NctsCustomsTR5MessagingProvider.New(header, string.Empty, null) as ITRCustomsMessagingProvider;

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
				var header = Factory.New<NctsHeader>();
				var provider = Phase5NctsCustomsTR5MessagingProvider.New(header, string.Empty, null);

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
			var header = Factory.New<NctsHeader>();
			var provider = Phase5NctsCustomsTR5MessagingProvider.New(header, string.Empty, null) as ISupportPreSendValidation;

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
