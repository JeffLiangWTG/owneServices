using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(BulkCommunication))]
	sealed class BulkCommunicationTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestTypeOfCall()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.TypeOfCall = "PHN";
			AssertEquals("PHN", comm.TypeOfCall);
		}

		public void TestCategory()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.Category = "ASX";
			AssertEquals("ASX", comm.Category);
		}

		public void TestStatus()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.Status = "SCH";
			AssertEquals("SCH", comm.Status);
		}

		public void TestSummary()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.Summary = "Campaign Items sent to contacts";
			AssertEquals("Campaign Items sent to contacts", comm.Summary);
		}

		public void TestStaffCoordinator()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.StaffCoordinator = "EO";
			AssertEquals("EO", comm.StaffCoordinator);
		}

		public void TestDuration()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.Duration = new ZDateTime(2014, 4, 4, 4, 4, 4);
			AssertEquals(new ZDateTime(2014, 4, 4, 4, 4, 4), comm.Duration);
		}

		public void TestShouldSendInvitation()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.ShouldSendInvitation = true;
			AssertEquals(true, comm.ShouldSendInvitation);
			comm.ShouldSendInvitation = false;
			AssertEquals(false, comm.ShouldSendInvitation);
		}

		public void TestOverallDisposition()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.Status = "COM";
			AssertEquals("CLS", comm.OverallDisposition);
			comm.Status = "SCH";
			AssertEquals("OPN", comm.OverallDisposition);
		}

		public void TestCallDate()
		{
			BulkCommunication comm = new BulkCommunication(Factory, Campaign);
			comm.CallDate = new ZDateTime(2014, 4, 4, 4, 4, 4);
			AssertEquals(new ZDateTime(2014, 4, 4, 4, 4, 4), comm.CallDate);
		}

		#endregion

		#region Calendar Reminder

		public void TestSendCalendarReminder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org1.Contacts.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org2.Contacts.AddNew();

			GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "SRP";
			salesRep.GS_EmailAddress = "info@wisetech.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			var bulkCommunication = new BulkCommunication(Factory, campaign);

			BulkCommunicationCollection collection = bulkCommunication.CommunicationCollection;
			collection.CreateFromCampaignItems(new GlbCompanyCampaignItem[] { campaignItem1, campaignItem2, campaignItem3 });

			AssertEquals("There should be three OrgSalesCall objects created", 3, collection.Count);

			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);

			OrgSalesCall call1 = collection[0];
			call1.ShouldSendInvitation = true;
			call1.OQ_NextCall = DateTime.Now.AddDays(1);
			call1.OQ_GS_NKSalesRep = "SRP";

			OrgSalesCall call2 = collection[1];
			call2.ShouldSendInvitation = false;

			OrgSalesCall call3 = collection[2];
			call3.ShouldSendInvitation = false;

			Factory.Save();
			bulkCommunication.SendCalenderReminder();

			AssertEquals("Invitation should be sent to call1", true, call1.IsInvitationSent);
			AssertEquals("No invitations sent to call2", false, call2.IsInvitationSent);
			AssertEquals("No invitations sent to call3", false, call3.IsInvitationSent);

			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			call3.ShouldSendInvitation = true;
			call3.OQ_NextCall = DateTime.Now.AddDays(3);
			Factory.Save();
			bulkCommunication.SendCalenderReminder();
			AssertEquals("Invitation should not be sent", false, call3.IsInvitationSent);

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact4 = org3.Contacts.AddNew();
			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			collection.CreateFromCampaignItems(new GlbCompanyCampaignItem[] { campaignItem4 });
			AssertEquals("There should be only one OrgSalesCall objects created", 1, collection.Count);
			OrgSalesCall call4 = collection[0];
			call4.ShouldSendInvitation = true;
			call4.OQ_NextCall = DateTime.Now.AddDays(1);
			call4.OQ_GS_NKSalesRep = "SRP";

			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			Factory.Save();
			bulkCommunication.SendCalenderReminder();
			AssertEquals("Invitation should not be sent", true, call4.IsInvitationSent);
		}

		#endregion

		public void TestCreateRelationshipForNewEntity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OCCC";
			var inquiry = Factory.New<SalesEnquiry>();
			var contact1 = org.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientID = inquiry.PK;
			campaignItem1.G8_RecipientTableCode = "O1";
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientID = contact1.PK;
			campaignItem2.G8_RecipientTableCode = "OC";

			Factory.Save();

			var communication = Factory.New<OrgSalesCall>();
			var communication1 = Factory.New<OrgSalesCall>();

			BulkCommunication.CreateRelationshipForNewEntity(communication, campaignItem1, (o) => { });
			AssertContainsExactElementsInAnyOrder("Communication Parent activity collection should contain CampaignItem1",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { campaignItem1 },
				communication.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());

			BulkCommunication.CreateRelationshipForNewEntity(communication1, campaignItem2, (o) => { });
			AssertContainsExactElementsInAnyOrder("Communication1 Parent activity collection should contain Campaign",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { campaignItem2 },
				communication1.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkCommunication(Factory, Campaign);
		}

		GlbCompanyCampaign Campaign;

		protected override void SetUp()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			base.SetUp();
		}

		#endregion
	}
}
