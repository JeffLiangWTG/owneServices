using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ExamResultsByRecipientFilterBusinessObject))]
	public class ExamResultsByRecipientFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCommencedFilter()
		{
			var filter = (ModuleDateFilter)ExamResultsFilter["Commenced"];
			AssertNotNull(filter);
			ExamAttempt.EXA_TestCommencedUtc = ZDateTime.UtcNow;
			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			ExamAttempt.EXA_G8 = campaignItem.PK;
			Factory.Save();
			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(3);
			var query = ExamResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow;
			filter.IsActive = true;
			query = ExamResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		public void TestCompletedFilter()
		{
			var filter = (ModuleDateFilter)ExamResultsFilter["Completed"];
			AssertNotNull(filter);
			ExamAttempt.EXA_TestCompletedUtc = ZDateTime.UtcNow;
			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			ExamAttempt.EXA_G8 = campaignItem.PK;
			Factory.Save();
			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(3);
			filter.IsActive = true;
			var query = ExamResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow;
			filter.IsActive = true;
			query = ExamResultsFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		#region Implementation
		GlbCompanyCampaign Campaign;
		ExamResultsByRecipientFilterBusinessObject ExamResultsFilter;
		ExamAttempt ExamAttempt;
		void CreateDataForTest()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ExamAttempt = Factory.NewWithValidTestData<ExamAttempt>();
			ExamAttempt.CampaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			ExamAttempt.CampaignItem.G8_RecipientID = ZGuid.NewZGuid();
			ExamResultsFilter = new ExamResultsByRecipientFilterBusinessObject();
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateDataForTest();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ExamResultsByRecipientFilterBusinessObject();
		}
		#endregion
	}
}
