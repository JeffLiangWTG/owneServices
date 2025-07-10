using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI.Testing
{
	public abstract class HRGlbCompanyCampaignContactFilterBusinessObjectTestBase : FilterStripBusinessObjectTestCase
	{
		public static void SwitchOffSubscriptionFilter(FilterStripBusinessObject filter)
		{
			filter[HRGlbCompanyCampaignContactFilterBusinessObject.InternalSubscriptionFilterCode].Visibility = FilterVisibility.Visible;
			filter[HRGlbCompanyCampaignContactFilterBusinessObject.InternalSubscriptionFilterCode].IsActive = false;
		}

		#region Implementation
		protected GlbStaff StaffMember;
		protected HRGlbCompanyCampaign Campaign;
		protected HRGlbCompanyCampaignContactFilterBusinessObject CampaignFilter;
		protected HRJobApplicant Applicant;
		protected GlbCampaignContactCollection Collection;
		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
		}

		protected void CreateContactsForTest()
		{
			StaffMember = Factory.NewWithValidTestData<GlbStaff>();
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Applicant.HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Residence;
			CampaignFilter = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			Collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRGlbCompanyCampaignContactFilterBusinessObject(Factory.NewWithValidTestData<HRGlbCompanyCampaign>());
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			base.GetFiltersExcludedFromSubgroupCheck();
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Driver Branch"));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			exclusions.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Driver Branch"));
			return exclusions;
		}

		public class HRGlbCompanyCampaignContactFilterBusinessObjectForTest : HRGlbCompanyCampaignContactFilterBusinessObject
		{
			public HRGlbCompanyCampaignContactFilterBusinessObjectForTest(HRGlbCompanyCampaign campaign) : base(campaign)
			{
			}

			public string StatusExposed(bool isActive)
			{
				return isActive ? StatusActive : StatusInactive;
			}
		}
		#endregion
	}
}
