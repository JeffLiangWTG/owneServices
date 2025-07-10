using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesRelationModel))]
	sealed class SalesRelationModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		#region RecentActivityDate

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2014, 1, 1)]
		public void TestRecentActivityDate()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			TestDateAttribute.Date = new DateTime(2014, 3, 1);
			Factory.Save();

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			TestDateAttribute.Date = new DateTime(2014, 4, 1);
			Factory.Save();

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			TestDateAttribute.Date = new DateTime(2014, 2, 1);
			Factory.Save();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode] = OrgContactSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID] = ZGuid.NewZGuid();
			Factory.Save();

			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot((ISalesRelationActivity)campaignItem);
			Factory.Save();

			CombineAssertions("RecentActivityDates", () =>
			{
				AssertEquals("opportunity1", new ZDateTime(2014, 4, 1, 10, 0, 0), opportunity1.SalesRelationModel.RecentActivityDate);
				AssertEquals("inquiry", new ZDateTime(2014, 4, 1, 10, 0, 0), inquiry.SalesRelationModel.RecentActivityDate);
				AssertEquals("opportunity2", new ZDateTime(2014, 2, 1, 10, 0, 0), opportunity2.SalesRelationModel.RecentActivityDate);
			});
		}

		#endregion

		#region HasSalesReation

		public void TestHasSalesReation()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			Factory.Save();

			AssertEquals(true, opportunity1.SalesRelationModel.HasSalesRelation);
			AssertEquals(true, inquiry.SalesRelationModel.HasSalesRelation);

			AssertEquals(false, opportunity2.SalesRelationModel.HasSalesRelation);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new SalesRelationModel(opportunity);
		}

		#endregion
	}
}
