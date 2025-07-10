using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(OrgContactCampaignReferences))]
	class OrgContactCampaignReferencesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasPostRelations()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var crossReference = new OrgContactCampaignReferences(contact);
			AssertEquals(false, crossReference.HasPostRelations);

			crossReference.CampaignChildren.Add(inquiry);
			AssertEquals(true, crossReference.HasPostRelations);

			crossReference.CampaignChildren.Clear();
			crossReference.CampaignItem = campaignItem;
			AssertEquals(false, crossReference.HasPostRelations);

			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(Factory.New<OrgOpportunity>());
			AssertEquals(true, crossReference.HasPostRelations);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgContactCampaignReferences(Factory.New<OrgContact>());
		}

		#endregion
	}
}
