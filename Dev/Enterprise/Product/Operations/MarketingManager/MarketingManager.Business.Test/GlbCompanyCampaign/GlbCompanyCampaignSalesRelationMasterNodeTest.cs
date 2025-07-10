using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSalesRelationMasterNodeTest : TestCaseWithFactory
	{
		public void TestGetChildren()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var campaignItemParent = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			campaignItemParent.RelatedChildActivityPivotCollection.AddNewPivot(campaignItem);
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			Factory.Save();

			AssertType(typeof(GlbCompanyCampaignSalesRelationMasterNode), campaign.SalesRelationModel.RootNode);
			var campaignNode = campaign.SalesRelationModel.RootNode;
			AssertEquals(campaign, campaignNode.BizObj);

			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication, opportunity }, campaignNode.ChildNodes.Select(node => node.BizObj));

			var communication2 = Factory.New<OrgSalesCall>();
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(communication2);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication, opportunity, communication2 }, campaignNode.ChildNodes.Select(node => node.BizObj));
		}

		public void TestGetChildren_AfterCampaignItemsSentChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication);

			AssertType(typeof(GlbCompanyCampaignSalesRelationMasterNode), campaign.SalesRelationModel.RootNode);
			var campaignNode = campaign.SalesRelationModel.RootNode;
			AssertEquals(campaign, campaignNode.BizObj);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication }, campaignNode.ChildNodes.Select(node => node.BizObj));

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication, opportunity }, campaignNode.ChildNodes.Select(node => node.BizObj));

			campaign.CampaignsItemsSent.RemoveAndDelete(campaignItem);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication }, campaignNode.ChildNodes.Select(node => node.BizObj));
		}

		public void TestGetChildren_TableHits()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			var campaignItemParent = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			for (var i = 0; i < 10; i++)
			{
				var campaignItemWithoutPostRelations = campaign.CampaignsItemsSent.AddNew();
				campaignItemWithoutPostRelations.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				campaignItemWithoutPostRelations.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			}

			Factory.Save();

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			campaignItemParent.RelatedChildActivityPivotCollection.AddNewPivot(campaignItem);
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var campaignInAnotherFactory = anotherFactory.Load<GlbCompanyCampaign>(campaign.PK);
			var accessChildNodes = campaignInAnotherFactory.SalesRelationModel.RootNode.ChildNodes;

			AssertMaxTableHits(3, GenPivotSchema.Constants.TableName, anotherFactory);
		}
	}
}
