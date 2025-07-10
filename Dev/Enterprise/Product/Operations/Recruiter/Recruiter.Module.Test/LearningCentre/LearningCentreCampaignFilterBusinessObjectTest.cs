using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(LearningCentreCampaignFilterBusinessObject))]
	public class LearningCentreCampaignFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTypeFilter()
		{
			LearningCentreCampaign examCampaign = Factory.New<LearningCentreCampaign>();
			LearningCentreCampaign scaledTestCampaign = Factory.New<LearningCentreCampaign>();
			scaledTestCampaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			LearningCentreCampaignCollection collection = new LearningCentreCampaignCollection(Factory);
			ModuleTextFilter typeFilter = (ModuleTextFilter)FilterBizO["Type"];
			typeFilter.Property = LearningCentreTestTypes.Codes.Exam;
			typeFilter.IsActive = true;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(examCampaign, collection);
			typeFilter.Property = LearningCentreTestTypes.Codes.Scaled;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(scaledTestCampaign, collection);
		}

		public void TestCoordinatorFilter()
		{
			ModuleNkFilter coordFilter = (ModuleNkFilter)FilterBizO["Campaign Coordinator"];
			Assert("This filter not supported on Web - yet", !coordFilter.IsPublishedOnWeb);
			AssertEquals(ModuleIDs.GlbStaff, coordFilter.ModuleId);
			AssertEquals(typeof(GlbStaffCollection), coordFilter.List.GetType());
		}

		public void TestCampaignFilters()
		{
			SetupTestDataForTestCampaignFilters();
			LearningCentreCampaignCollection collection = new LearningCentreCampaignCollection(Factory);
			AssertEquals("Precondition", 7, collection.Count);
			ModuleNkFilter mgrFilter = (ModuleNkFilter)FilterBizO["Campaign Manager"];
			mgrFilter.Property = "C";
			mgrFilter.IsActive = true;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertContainCampaigns(collection, "MEH001", "MEH002", "CRT004");
			ModuleNkFilter coordFilter = (ModuleNkFilter)FilterBizO["Campaign Coordinator"];
			coordFilter.Property = "E";
			coordFilter.IsActive = true;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertContainCampaigns(collection, "MEH001", "MEH002");
			ModuleTextFilter idFilter = (ModuleTextFilter)FilterBizO["Campaign ID"];
			idFilter.IsActive = true;
			idFilter.Property = "CRT005";
			mgrFilter.IsActive = false;
			coordFilter.IsActive = false;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertContainCampaigns(collection, "CRT005");
			ModuleTextFilter stageFilter = (ModuleTextFilter)FilterBizO["Stage"];
			stageFilter.Property = "INP";
			stageFilter.IsActive = true;
			idFilter.IsActive = false;
			collection.AdditionalFilter = FilterBizO.Filter;
			Assert("This filter not supported on Web", !stageFilter.IsPublishedOnWeb);
			AssertContainCampaigns(collection, "CRT001", "CRT003", "CRT005");
			ModuleTextFilter commentFilter = (ModuleTextFilter)FilterBizO["Comment"];
			commentFilter.Property = "comment 4";
			commentFilter.IsActive = true;
			stageFilter.IsActive = false;
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertContainCampaigns(collection, "CRT002");
		}

		void SetupTestDataForTestCampaignFilters()
		{
			LearningCentreCampaign certExam1 = NewCertificationExam("CRT001", "exam 1", "ZZ", "E", "INP", "comment 1");
			LearningCentreCampaign certExam2 = NewCertificationExam("MEH001", "exam 2", "C", "E", "CLO", "comment 2");
			LearningCentreCampaign certExam3 = NewCertificationExam("MEH002", "exam 3", "C", "E", "NYS", "comment 3");
			LearningCentreCampaign certExam4 = NewCertificationExam("CRT002", "exam 4", "E", "E", "CLO", "comment 4");
			LearningCentreCampaign certExam5 = NewCertificationExam("CRT003", "exam 5", "E", "E", "INP", "comment 5");
			LearningCentreCampaign certExam6 = NewCertificationExam("CRT004", "exam 6", "C", "ZZ", "CLO", "comment 6");
			LearningCentreCampaign certExam7 = NewCertificationExam("CRT005", "exam 7", "ZZ", "C", "INP", "comment 7");
			Factory.Save();
		}

		LearningCentreCampaign NewCertificationExam(string campaignID, string campaignName, string campaignManager, string campaignCoordinator, string stage, string comment)
		{
			LearningCentreCampaign result = Factory.NewWithValidTestData<LearningCentreCampaign>();
			result.G0_CampaignID = campaignID;
			result.G0_CampaignName = campaignName;
			result.G0_GS_NKCampaignManager = campaignManager;
			result.G0_GS_NKCampaignCoordinator = campaignCoordinator;
			result.G0_Stage = stage;
			result.G0_CampaignComment = comment;
			return result;
		}

		void AssertContainCampaigns(LearningCentreCampaignCollection collection, params string[] campaignIDs)
		{
			AssertEquals(campaignIDs.Length, collection.Count);
			AssertEquals(campaignIDs.Length, new List<LearningCentreCampaign>(collection.Find(new ZQuery(GlbCompanyCampaignSchema.G0_CampaignID, campaignIDs))).Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LearningCentreCampaignFilterBusinessObject();
		}

		LearningCentreCampaignFilterBusinessObject FilterBizO
		{
			get
			{
				return (LearningCentreCampaignFilterBusinessObject)CachedBusinessObject;
			}
		}
	}
}
