using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UnsubscribeContactsBusinessObject))]
	class UnsubscribeContactsBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnsubscribeContactsBusinessObjectForTest(Factory);
		}

		public void TestALLTypeSecurity()
		{
			AssertALLTypeSecurity();
		}

		public void TestALLTypeSecurity_WithEmptyRegistrySettings()
		{
			var emptyList = new CodeDescriptionBoolCollection();
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			AssertALLTypeSecurity();
		}

		void AssertALLTypeSecurity()
		{
			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var bizo = (UnsubscribeContactsBusinessObjectForTest)GetNewBusinessObject();
				var sub = new SubscriptionProperties();
				bizo.OnNewSubscriptionAdded_Exposed(sub, EventArgs.Empty);

				AssertEquals(false, sub.Lookups.MediaCategoryWithAllList.ContainsCode(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode));
				AssertEquals(false, sub.Lookups.MediaTypeWithAllList.ContainsCode(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode));
				AssertNotEquals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode, sub.MediaCategoryWithAll);
				AssertNotEquals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode, sub.MediaTypeWithAll);
			}
		}
	}
}
