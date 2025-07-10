using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignClick))]
	sealed class GlbCompanyCampaignClickTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2015, 5, 12, 10, 0, 0)]
		public void TestProperties()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EDW";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";  // -7 hours
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = true;

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "CSE";
				org.OH_FullName = "Computer Society";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "x@google.com";
				contact.OC_ContactName = "Bob John";

				var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				campaign.G0_CampaignName = "Test Campaign";

				var campaignItem = campaign.CampaignsItemsSent.AddNew();
				campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				campaignItem.G8_RecipientID = contact.PK;

				var link = campaign.TrackedLinks.AddNew();
				link.GCL_URL = "http://www.yahoo.com";
				link.GCL_Context = "Yahoo Plus";
				link.GCL_IsTracked = true;
				link.GCL_IsImage = false;

				var click = NewCampaignItem();
				click.GCC_G8_Recipient = campaignItem.PK;
				click.GCC_GCL = link.PK;
				click.GCC_ClickTimeUtc = ZDateTime.UtcNow;

				Factory.Save();

				AssertEquals("Test Campaign", click.CampaignName);
				AssertEquals("Bob John", click.ContactName);
				AssertEquals("x@google.com", click.ContactEmail);
				AssertEquals("CSE", click.OrgCode);
				AssertEquals("Computer Society", click.OrgName);
				AssertEquals(true, click.IsTracking);
				AssertEquals("Yahoo Plus", click.TrackingContext);
				AssertEquals(false, click.IsImage);
				AssertEquals("http://www.yahoo.com", click.DestinationURL);
				AssertEquals(click.GCC_ClickTimeUtc.AddHours(-7), click.LocalActivityTime);
			}
		}

		public void TestSaveCallsTransition()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_G0 = campaign.PK;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();
			var click = Factory.NewWithValidTestData<GlbCompanyCampaignClickForTest>();
			var link = Factory.NewWithValidTestData<GlbCompanyCampaignLinkForTest>();
			link.GCL_G0_Campaign = campaign.PK;
			click.GCC_GCL = link.PK;
			click.GCC_G8_Recipient = item.PK;

			AssertEquals(0, campaign.TransitionCounter);

			Factory.Save();

			AssertEquals(1, campaign.TransitionCounter);
		}

		#region Implementation

		GlbCompanyCampaignClick NewCampaignItem()
		{
			return (GlbCompanyCampaignClick)Factory.New(GetExpectedBusinessObjectType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = (GlbCompanyCampaignClick)base.GetNewBusinessObject();
			var link = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			bizo.GCC_GCL = link.PK;
			return bizo;
		}

		class GlbCompanyCampaignClickForTest : GlbCompanyCampaignClick
		{
			public GlbCompanyCampaignClickForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override GlbCompanyCampaignLink Link
			{
				get { return Factory.Load<GlbCompanyCampaignLinkForTest>(GCC_GCL); }
			}
		}

		class GlbCompanyCampaignLinkForTest : GlbCompanyCampaignLink
		{
			public GlbCompanyCampaignLinkForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override GlbCompanyCampaign Campaign
			{
				get { return Factory.Load<GlbCompanyCampaignForTest>(GCL_G0_Campaign); }
			}
		}

		class GlbCompanyCampaignForTest : GlbCompanyCampaign
		{
			public GlbCompanyCampaignForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override List<Tuple<ZGuid, TransitionStatus>> TransitionAndSchedule(IEnumerable<ZGuid> transferItemPK, List<String> transitionScheduleErrorList = null)
			{
				TransitionCounter++;
				return new List<Tuple<ZGuid, TransitionStatus>>();
			}

			public int TransitionCounter;
		}

		#endregion
	}
}
