using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.MessagingProcess.Testing
{
	sealed class CusEntryHeaderCustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var provider = CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, TRMessageTypes.Codes.DTE, null);
			AssertType<CusEntryHeaderCustomsMessagingProvider>(provider);

			var trCommonProvider = provider as ITRCustomsMessagingProvider;
			AssertNotNull("Is ITRCustomsMessagingProvider", trCommonProvider);
		}

		public void TestGetMessengers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var provider = CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, TRMessageTypes.Codes.DTE, null);

			var messenger = provider.GetMessengers().Single();
			AssertType<TRCustomsMessenger>(messenger);
			var generator = messenger.MessageGenerator;
			AssertType<CusEntryHeaderRegistryMessageGenerator>(generator);
		}

		public void TestGetMessengers_NotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertExceptionThrown<NotSupportedException>("Invalid message type", () => CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, "CRASH", null));
		}

		public void TestITRCustomsMessagingProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var trProvider = CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, TRMessageTypes.Codes.DTE, null) as ITRCustomsMessagingProvider;

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
				var declaration = Factory.New<JobDeclaration>();
				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

				var provider = CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, TRMessageTypes.Codes.DTE, null);

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
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var provider = CusEntryHeaderCustomsMessagingProvider.New(cusEntryHeader, TRMessageTypes.Codes.DTE, null) as ISupportPreSendValidation;

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
