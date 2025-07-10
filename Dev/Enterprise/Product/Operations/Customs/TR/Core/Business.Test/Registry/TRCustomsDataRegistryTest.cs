using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.TR.Business.TRCustomsDataRegistry;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRCustomsDataRegistry))]
	class TRCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<TRCustomsDataRegistry>
	{
		public void TestStampDutyLedgerNumberCustomization()
		{
			TestGenericRegistryItem(
				ItemSet.StampDutyLedgerNumberCustomization,
				"TRStampDutyLedgerNumberCustomization",
				Categories.Customs_Turkey,
				"Stamp Duty Ledger Number Customization",
				"Override this value to customize how Stamp Duty Ledger numbers are formatted",
				RegistryStorageFlags.Company,
				RegistryOptions.Default
			);
			AssertEquals(RegistryItemSet.CountryFilterPKs.Turkey, ItemSet.StampDutyLedgerNumberCustomization.CountryFilterPKs);
		}

		public void TestEnableTRNCTS()
		{
			TestRegistryItem(ItemSet.EnableTRNCTS, "EnableTRNCTS", TRCustomsDataRegistry.Categories.Customs_Turkey_NCTS, "Enable Turkey NCTS", "Enable Turkey NCTS?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestTRTestingSystem()
		{
			TestRegistryItem(
				ItemSet.TRTestingSystem,
				"IsTRTesting",
				Categories.Customs_Turkey,
				"Is TR Testing System?",
				"TR messages be sent to the test rather than production system?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsTRTestingSystem()
		{
			Instance.TRTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(Instance.IsTRTestingSystem);
			Instance.TRTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!Instance.IsTRTestingSystem);
		}

		public void TestExposeETradeModule()
		{
			TestGenericRegistryItem(ItemSet.ExposeETradeModule,
									"ExposeETradeModule",
									TRCustomsDataRegistry.Categories.Customs_Turkey,
									"Expose E Trade Module?",
									"Expose E Trade Module?",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForDevelopers);

			AssertEquals("Default value", false, ItemSet.ExposeETradeModule.Value);
		}

		public void TestExposeSimplifiedProcedureTransitSystemModule()
		{
			TestGenericRegistryItem(ItemSet.ExposeSimplifiedProcedureTransitSystemModule,
									"ExposeSimplifiedProcedureTransitSystemModule",
									TRCustomsDataRegistry.Categories.Customs_Turkey,
									"Expose Simplified Procedure Transit System Module?",
									"Expose Simplified Procedure Transit System Module?",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForDevelopers);

			AssertEquals("Default value", false, ItemSet.ExposeSimplifiedProcedureTransitSystemModule.Value);
		}

		public void TestTRMANGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.TRMANGroupNotification,
				"TRMANGroupNotification",
				Categories.Customs_Turkey_Manifest,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default);
		}

		public void TestSendTRNCTSMessageWithoutSign()
		{
			TestRegistryItem(ItemSet.SendTRNCTSMessageWithoutSign, "SendTRNCTSMessageWithoutSign", TRCustomsDataRegistry.Categories.Customs_Turkey_NCTS, "Send NCTS Message Without E-signature", "Send NCTS Message Without E-signature?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, false);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		public void TestFTPSettings()
		{
			TestGenericRegistryItem(ItemSet.FTPSettings,
				"ExportUnionFTPSettings",
				Categories.Customs_Turkey,
				"Export Union FTP Settings",
				"The registry item stores the FTP settings for sending messages to the Export Union.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport);
		}

		public void TestSendTRDeclarationWithComma()
		{
			TestRegistryItem(ItemSet.SendTRDeclarationWithComma, "SendTRDeclarationWithComma", TRCustomsDataRegistry.Categories.Customs_Turkey_Declaration, "Use comma in Ratio field in Declaration message", "Use comma in Ratio field in Declaration message?", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestNCTSPhase5Credentials()
		{
			TestGenericRegistryItem(ItemSet.NCTSPhase5Credentials,
				"NCTSPhase5Credentials",
				Categories.Customs_Turkey_NCTS,
				"NCTS Phase 5 Credentials",
				"NCTS Phase 5 Credentials",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport);
		}

		public void TestFTPSettingsRegistryItemProperties()
		{
			var item = ItemSet.FTPSettings;
			CombineAssertions(() =>
			{
				AssertEquals("OnUpdateAction", ItemSet.RegistryActionHandler.OnUpdateAction, item.OnUpdateAction);
				AssertEquals("OnAllValuesSavedAction", ItemSet.RegistryActionHandler.OnAllValuesSaved, item.OnAllValuesSavedAction);
				AssertEquals("OnAllValuesSavedAction", CountryFilterPKs.Turkey, item.CountryFilterPKs);
			});
		}
	}
}
