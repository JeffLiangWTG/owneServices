using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobOpeningsFilterBusinessObject))]
	public class HRJobOpeningsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected ModuleNkFilter GetModuleNkFilter(ZString description)
		{
			return (ModuleNkFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestCampaignLeader()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			Campaign1.HV_GS_NKControlledBy = staff1.GS_Code;
			Campaign2.HV_GS_NKControlledBy = staff2.GS_Code;
			Factory.Save();
			ModuleNkFilter filter = GetModuleNkFilter("Campaign Leader");
			filter.Property = staff1.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property = staff2.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			var staff3 = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			filter.Property = staff3.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		protected ModuleGuidFilter GetModuleGuidFilter(ZString description)
		{
			return (ModuleGuidFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestClientAccount()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Campaign1.HV_OH_ClientAccount = org1.PK;
			Campaign2.HV_OH_ClientAccount = org2.PK;
			Factory.Save();
			ModuleGuidFilter filter = GetModuleGuidFilter("Client");
			filter.Property = org1.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property = org2.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			filter.Property = org3.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		public void TestLocation()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address1 = org1.Addresses.AddNew();
			address1.FillWithValidTestData();
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address2 = org2.Addresses.AddNew();
			address2.FillWithValidTestData();
			address2.OA_RL_NKRelatedPortCode = "AUMEL";
			Campaign1.HV_OH_ClientAccount = org1.PK;
			Campaign1.HV_OA_ClientAddress = address1.PK;
			Campaign2.HV_OH_ClientAccount = org2.PK;
			Campaign2.HV_OA_ClientAddress = address2.PK;
			Factory.Save();
			ModuleNkFilter filter = GetModuleNkFilter("Location");
			filter.IsActive = true;
			filter.Property = "AUSYD";
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property = "AUMEL";
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property = "AU";
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property = "US";
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		public void TestAdStartDate()
		{
			Campaign1.HV_CampaignStartDate = new ZDateTime(2021, 8, 2);
			Campaign2.HV_CampaignStartDate = new ZDateTime(2021, 8, 9);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)(GetNewFilterStripBusinessObject()["Ad Start Date"]);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2021, 8, 1);
			filter.Property2 = new ZDateTime(2021, 8, 8);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 8, 7);
			filter.Property2 = new ZDateTime(2021, 8, 12);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 8, 10);
			filter.Property2 = new ZDateTime(2021, 8, 12);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 7, 31);
			filter.Property2 = new ZDateTime(2021, 8, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		public void TestAdEndDate()
		{
			Campaign1.HV_CampaignEndDate = new ZDateTime(2021, 8, 2);
			Campaign2.HV_CampaignEndDate = new ZDateTime(2021, 8, 9);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)(GetNewFilterStripBusinessObject()["Ad End Date"]);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2021, 8, 1);
			filter.Property2 = new ZDateTime(2021, 8, 8);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 8, 7);
			filter.Property2 = new ZDateTime(2021, 8, 12);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 8, 10);
			filter.Property2 = new ZDateTime(2021, 8, 12);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property1 = new ZDateTime(2021, 7, 31);
			filter.Property2 = new ZDateTime(2021, 8, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		public void TestAdStartAndEndDate()
		{
			HRRecruitmentJobCampaign campaign3 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Campaign1.HV_CampaignStartDate = new ZDateTime(2021, 8, 2);
			Campaign2.HV_CampaignStartDate = new ZDateTime(2021, 8, 5);
			campaign3.HV_CampaignStartDate = new ZDateTime(2021, 8, 8);
			Campaign1.HV_CampaignEndDate = new ZDateTime(2021, 9, 2);
			Campaign2.HV_CampaignEndDate = new ZDateTime(2021, 9, 5);
			campaign3.HV_CampaignEndDate = new ZDateTime(2021, 9, 8);
			Factory.Save();
			ModuleDateFilter filterStart = (ModuleDateFilter)(GetNewFilterStripBusinessObject()["Ad Start Date"]);
			filterStart.IsActive = true;
			filterStart.Property1 = new ZDateTime(2021, 8, 3);
			filterStart.Property2 = new ZDateTime(2021, 8, 9);
			filterStart.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ModuleDateFilter filterEnd = (ModuleDateFilter)(GetNewFilterStripBusinessObject()["Ad End Date"]);
			filterEnd.IsActive = true;
			filterEnd.Property1 = new ZDateTime(2021, 9, 1);
			filterEnd.Property2 = new ZDateTime(2021, 9, 6);
			filterEnd.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var combinedQuery = filterStart.Query.AddToFilter(filterEnd.Query);
			Collection.Load(combinedQuery);
			AssertCollectionContains(Campaign2, Collection);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(campaign3, Collection);
		}

		public void TestJobRole()
		{
			HRJobRole role1 = Factory.NewWithValidTestData<HRJobRole>();
			HRJobRole role2 = Factory.NewWithValidTestData<HRJobRole>();
			Campaign1.HV_HJ_JobRole = role1.PK;
			Campaign2.HV_HJ_JobRole = role2.PK;
			Factory.Save();
			ModuleGuidFilter filter = GetModuleGuidFilter("Job Role");
			filter.Property = role1.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
			filter.Property = role2.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Campaign1, Collection);
			AssertCollectionContains(Campaign2, Collection);
			HRJobRole role3 = Factory.NewWithValidTestData<HRJobRole>();
			Factory.Save();
			filter.Property = role3.PK;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Campaign1, Collection);
			AssertCollectionNotContains(Campaign2, Collection);
		}

		#region Implementation
		protected HRRecruitmentJobCampaign Campaign1, Campaign2;
		protected HRRecruitmentJobCampaignCollection Collection;
		protected override void SetUp()
		{
			base.SetUp();
			Collection = new HRRecruitmentJobCampaignCollection(Factory);
			Campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobOpeningsFilterBusinessObject();
		}
		#endregion
	}
}
