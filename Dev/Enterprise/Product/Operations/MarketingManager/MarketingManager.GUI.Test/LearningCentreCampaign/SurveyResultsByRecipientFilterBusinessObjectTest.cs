using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SurveyResultsByRecipientFilterBusinessObject))]
	public class SurveyResultsByRecipientFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestClosedDateFilter()
		{
			var filter = (ModuleDateFilter)SurveyResultsFilter["Closed Date"];
			AssertNotNull(filter);

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;

			Factory.Save();

			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(3);

			var query = SurveyResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow;
			filter.IsActive = true;

			query = SurveyResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		public void TestSenderStaffNameFilter()
		{
			var filter = (ModuleTextFilter)SurveyResultsFilter["Sender Staff Name"];
			AssertNotNull(filter);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test Name";
			staff.GS_Code = "TSN";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_SystemCreateUser = staff.GS_Code;

			Factory.Save();

			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);

			filter.Property = "not a name";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var query = SurveyResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			filter.Property = staff.GS_FullName;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			query = SurveyResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		#region Implementation

		GlbCompanyCampaign Campaign;
		SurveyResultsByRecipientFilterBusinessObject SurveyResultsFilter;

		void CreateDataForTest()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			SurveyResultsFilter = new SurveyResultsByRecipientFilterBusinessObject();

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateDataForTest();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SurveyResultsByRecipientFilterBusinessObject();
		}

		#endregion
	}
}
