using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public abstract class GlbCompanyCampaignContactFilterBusinessObjectTestBase : GlbCompanyCampaignContactFilterTestCaseWithSubGroupCheckExclusions
	{
		public static void SwitchOffSubscriptionFilter(FilterStripBusinessObject filter)
		{
			filter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode].Visibility = FilterVisibility.Visible;
			filter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode].IsActive = false;
		}

		#region Implementation

		protected OrgHeader organisation;
		protected GlbCompanyCampaign Campaign;
		protected GlbCompanyCampaignContactFilterBusinessObject CampaignFilter;
		protected OrgContact Contact;

		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
		}

		protected void CreateContactsForTest()
		{
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			CampaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCompanyCampaignContactFilterBusinessObject(Factory.NewWithValidTestData<GlbCompanyCampaign>());
		}

		#endregion
	}
}
