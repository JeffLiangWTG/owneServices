using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.UY.Manifest.Business.UYCustomsDataRegistry;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(UYCustomsDataRegistry))]
	class UYCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<UYCustomsDataRegistry>
	{
		public void TestUYTestingSystem()
		{
			TestRegistryItem(
				ItemSet.UYTestingSystem,
				"IsUYTesting",
				Categories.Customs_Uruguay,
				"Is Test Mode?",
				"Should UY messages be sent to Test System rather than Production System?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsUYTestingSystem()
		{
			Instance.UYTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(Instance.IsUYTestingSystem);
			Instance.UYTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!Instance.IsUYTestingSystem);
		}

		public void TestUYMANGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.UYMANGroupNotification,
				"UYMANGroupNotification",
				Categories.Customs_Uruguay_Manifest,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		public void TestRegistryOptionsByDefault()
		{
			AssertEquals(RegistryOptions.IsOnlyForSupport, UYCustomsDataRegistry.Instance.UYTestingSystem.Options);
			AssertEquals(RegistryOptions.Default, UYCustomsDataRegistry.Instance.UYMANGroupNotification.Options);
		}
	}
}
