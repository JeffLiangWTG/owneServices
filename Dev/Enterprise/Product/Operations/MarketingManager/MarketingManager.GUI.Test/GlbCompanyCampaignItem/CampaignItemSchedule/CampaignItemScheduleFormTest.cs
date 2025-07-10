using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignItemScheduleForm))]
	public class CampaignItemScheduleFormTest : ZFormBasherTest
	{
		public void TestSchedulingCampaign()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_R3 = timeZoneSet.PK;

			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.UtcNow;

			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			Factory.Save();

			var campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);
			var contacts = new LinkedHashSet<IScheduleItemsProvider>();
			contacts.UnionWith(campaignContactCollection.Cast<IScheduleItemsProvider>());

			using (CampaignItemScheduleForm form = new CampaignItemScheduleForm(campaign, contacts))
			{
				AssertEquals("3 contacts should be added for scheduling", 3, campaign.CampaignItemSchedule.SelectedScheduleItems.Count);
				form.Show();
				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CampaignItemSchedule_SendScheduleEventHandler(null, new GlbCompanyCampaignItemSchedule.SendScheduleEventArgs(new Collection<CampaignContact>(contacts.Cast<CampaignContact>().ToList())));
				AssertEquals("Last message should be", "You are about to schedule delivery to 3 contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, campaign.CampaignsItemsSent.Count);
				Assert("3 new campaignItems should all have a scheduled time", campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().All(s => s.G8_ScheduleTimeUtc != default(ZDateTime)));
				Assert("3 new campaignItems should all have a Queued Status code", campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().All(s => s.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE));
			}
		}

		public void TestFormDataSourceIsReadOnly()
		{
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			Factory.Save();
			Collection<IScheduleItemsProvider> contacts = new Collection<IScheduleItemsProvider>();
			using (CampaignItemScheduleForm form = new CampaignItemScheduleForm(campaign, contacts))
			{
				AssertEquals("No contacts should be added for scheduling", 0, campaign.CampaignItemSchedule.SelectedScheduleItems.Count);
				form.Show();
				Assert("Form's datasource is readonly", campaign.ReadOnly);
				Assert("Child of datasource is readonly", campaign.CampaignItemSchedule.ReadOnly);
			}

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "EXEC";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact a";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = "UNV";
			Factory.Save();

			Collection<IScheduleItemsProvider> items = new Collection<IScheduleItemsProvider>();
			items.Add(campaignItem);

			campaign.SetReadOnlyIncludingChildren(false);
			ScheduleCampaignItems scheduleCampaignItem = new ScheduleCampaignItems(campaign);
			scheduleCampaignItem.Status = "SNT";
			campaign.CampaignItemSchedule.ScheduleItemsCollection.Add(scheduleCampaignItem);
			using (CampaignItemScheduleForm form = new CampaignItemScheduleForm(campaign, items))
			{
				AssertEquals("One item should be added for scheduling", 1, campaign.CampaignItemSchedule.SelectedScheduleItems.Count);
				form.Show();
				Assert("Form's datasource is readonly because item does not have QUE status", campaign.ReadOnly);
				Assert("Child of datasource is readonly", campaign.CampaignItemSchedule.ReadOnly);
			}

			campaign.SetReadOnlyIncludingChildren(false);
			campaign.CampaignItemSchedule.ScheduleItemsCollection.RemoveAll();
			scheduleCampaignItem.Status = "QUE";
			campaign.CampaignItemSchedule.ScheduleItemsCollection.Add(scheduleCampaignItem);
			using (CampaignItemScheduleForm form = new CampaignItemScheduleForm(campaign, items))
			{
				AssertEquals("One item should be added for scheduling", 1, campaign.CampaignItemSchedule.SelectedScheduleItems.Count);
				form.Show();
				Assert("Form's datasource is NOT readonly because item has QUE status", !campaign.ReadOnly);
				Assert("Child of datasource is NOT readonly", !campaign.CampaignItemSchedule.ReadOnly);
			}
		}

		public void TestSchedulingShowsProgressForm()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO2";
			unloco.RL_R3 = timeZoneSet.PK;

			timeZoneSet.R3_R2_StandardZone = timeZone.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			org.OH_RL_NKClosestPort = unloco.RL_Code;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "CC";
			contact3.OC_Email = "c@b.net";

			ZDateTime time = ZDateTime.UtcNow;

			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			Factory.Save();

			var campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);
			var contacts = new LinkedHashSet<IScheduleItemsProvider>();
			contacts.UnionWith(campaignContactCollection.Cast<IScheduleItemsProvider>());

			using (CampaignItemScheduleForm form = new CampaignItemScheduleForm(campaign, contacts))
			{
				form.Show();
				form.FireSaveButton();
				AssertNull(form.LastSendProgressForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CampaignItemSchedule_SendScheduleEventHandler(null, new GlbCompanyCampaignItemSchedule.SendScheduleEventArgs(new Collection<CampaignContact>(contacts.Cast<CampaignContact>().ToList())));
				AssertNotNull(form.LastSendProgressForm);
				Assert(form.LastSendProgressForm.IsDisposed);
			}
		}

		RefTimeZoneSet timeZoneSet;
		RefTimeZone timeZone;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.R3_TimeZoneSetName = "Eastern";

			timeZone = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "EST";
			timeZone.R2_OffsetMinutesFromUTC = 600;
		}

		GlbCompanyCampaignTestHelper Helper
		{
			get { return helper ?? (helper = new GlbCompanyCampaignTestHelper(Factory)); }
		}

		GlbCompanyCampaignTestHelper helper;

		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new CampaignItemScheduleForm(campaign, new Collection<IScheduleItemsProvider>());
		}

		#endregion
	}
}
