using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ScheduleUserControlTest : TestCaseWithFactory
	{
		public void TestHideOrShowStatusColumn()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItemSchedule scheduler = campaign.CampaignItemSchedule;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SETE";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "eddie@test.com";
			contact.OC_ContactName = "Eddie";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = new GlbCampaignContactCollection(campaign);
			campaignContactCollection.Load(new ZQuery(ViewCampaignContactSchema.PK, contact.PK));

			AssertEquals(1, campaignContactCollection.Count);

			scheduler.SelectedScheduleItems.Add(campaignContactCollection.Cast<CampaignContact>().First());
			using (FormForTest form = new FormForTest(scheduler))
			{
				form.Show();
				AssertEquals(null, form.ScheduleControl.RecipientTimeZonesGridExposed.Columns.FirstOrDefault(c => c.ColumnName == "StatusText"));
			}

			scheduler.SelectedScheduleItems.Clear();
			scheduler.SelectedScheduleItems.Add(campaignItem);
			using (FormForTest form = new FormForTest(scheduler))
			{
				form.Show();
				AssertNotNull(form.ScheduleControl.RecipientTimeZonesGridExposed.Columns.FirstOrDefault(c => c.ColumnName == "StatusText"));
			}
		}

		#region Implementation

		public class FormForTest : ZForm
		{
			public FormForTest(GlbCompanyCampaignItemSchedule businessEntity)
				: base(businessEntity)
			{
				ScheduleControl = new ScheduleUserControlForTest();
				Controls.Add(ScheduleControl);
				ScheduleControl.SetDataBinding(businessEntity, "");
			}

			public ScheduleUserControlForTest ScheduleControl;
		}

		public class ScheduleUserControlForTest : ScheduleUserControl
		{
			public ScheduleUserControlForTest() : base() { }

			internal ZArchitecture.ZGrid RecipientTimeZonesGridExposed
			{
				get { return base.RecipientTimeZonesGrid; }
			}
		}

		#endregion
	}
}
