using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaign))]
	sealed class HRRecruitmentJobCampaignTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCustomBusinessObject()
			=> AssertNotNull(Factory.New<HRRecruitmentJobCampaign>().GetCustomBusinessObject());

		public void TestGetWorkflowInformationProvider()
			=> AssertNull(Factory.New<HRRecruitmentJobCampaign>().GetWorkflowInformationProvider());

		public void TestWorkflowItems()
		{
			var collection = Factory.New<HRRecruitmentJobCampaign>().WorkflowItems;
			AssertNotNull("WorkflowItems collection should not be null", collection);
			AssertNotNull(collection);
		}

		public void TestWorkflowType()
			=> AssertEquals("HRJ", Factory.New<HRRecruitmentJobCampaign>().WorkflowType);

		public void TestJobRole()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			AssertNull("Job Role should not be set", campaign.JobRole);

			HRJobRole role = Factory.New<HRJobRole>();
			campaign.HV_HJ_JobRole = role.PK;

			AssertEquals("Role should be set", role, campaign.JobRole);
		}

		public void TestSetDefaultValues()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			Assert("Client Address should be readonly", campaign.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should be readonly", campaign.HV_OC_ClientContactInfo.ReadOnly);
		}

		public void TestOnLoaded()
		{
			HRRecruitmentJobCampaign campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRRecruitmentJobCampaign campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			campaign1.HV_OH_ClientAccount = org.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			var loadedCampaign1 = newFactory.Load<HRRecruitmentJobCampaign>(campaign1.PK);
			Assert("Client Address should NOT be readonly", !loadedCampaign1.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should NOT be readonly", !loadedCampaign1.HV_OC_ClientContactInfo.ReadOnly);

			HRRecruitmentJobCampaign loadedCampaign2 = newFactory.Load<HRRecruitmentJobCampaign>(campaign2.PK);
			Assert("Client Address should be readonly", loadedCampaign2.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should be readonly", loadedCampaign2.HV_OC_ClientContactInfo.ReadOnly);
		}

		public void TestIsAutologged()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();
			Assert("Campaign should contain logs", campaign.Logs.GetAllLogs().Count > 0);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			AssertEquals("Should be 0 business objects with related logs", 0, campaign.BusinessObjectsWithRelatedEvents.Length);

			campaign.Applications.AddNew();
			AssertEquals("Should be 1 business object with related logs", 1, campaign.BusinessObjectsWithRelatedEvents.Length);

			campaign.Applications.AddNew();
			AssertEquals("Should be 2 business objects with related logs", 2, campaign.BusinessObjectsWithRelatedEvents.Length);

			campaign.Applications[0].Interviews.AddNew();
			AssertEquals("Should be 3 business objects with related logs", 3, campaign.BusinessObjectsWithRelatedEvents.Length);

			campaign.AdPlacements.AddNew();
			AssertEquals("Should be 4 business objects with related logs", 4, campaign.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			AssertEquals("Should be 0 business objects with related notes", 0, campaign.BusinessObjectsWithRelatedNotes.Length);

			campaign.Applications.AddNew();
			AssertEquals("Should be 1 business object with related notes", 1, campaign.BusinessObjectsWithRelatedNotes.Length);

			campaign.Applications.AddNew();
			AssertEquals("Should be 2 business objects with related notes", 2, campaign.BusinessObjectsWithRelatedNotes.Length);

			campaign.Applications[0].Interviews.AddNew();
			AssertEquals("Should be 3 business objects with related notes", 3, campaign.BusinessObjectsWithRelatedNotes.Length);

			campaign.AdPlacements.AddNew();
			AssertEquals("Should be 4 business objects with related notes", 4, campaign.BusinessObjectsWithRelatedNotes.Length);
		}

		public void TestApplications()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobApplication application = Factory.New<HRJobApplication>();
			application.HP_HV = campaign.PK;
			Assert("Applications collection should contain application", campaign.Applications.Contains(application));
		}

		public void TestAdPlacements()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobAdPlacement adPlacement = Factory.New<HRJobAdPlacement>();
			adPlacement.HQ_HV = campaign.PK;
			Assert("Ad Placement collection should contain ad placement", campaign.AdPlacements.Contains(adPlacement));
		}

		public void TestFillWithValidTestDataCore()
		{
			var campaignWithApplications = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>(TestBusinessObjectKind.PopulateDependentCollections);
			Assert("Campaign should have at least one application", campaignWithApplications.Applications.Count > 0);
			Assert("Application should be for a valid applicant", campaignWithApplications.Applications[0].HP_HA.IsValid);

			var campaignWithNoApplications = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>(TestBusinessObjectKind.MinimumRequiredToSave);
			AssertEquals("Campaign should not have any applications", 0, campaignWithNoApplications.Applications.Count);
		}

		public void TestDelete()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			HRJobAdPlacement adPlacement = campaign.AdPlacements.AddNew();
			campaign.Delete();

			Assert("Application should be deleted", application.IsDeleted);
			Assert("Ad Placement should be deleted", adPlacement.IsDeleted);
		}

		public void TestImplementsIDocManagerSupport()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(HRRecruitmentJobCampaign)));
			AssertEquals("HRRecruitmentJobCampaign DocManagerCode = CAM", "CAM", campaign.DocManagerInfo.DocManagerCode);
			AssertEquals("DocManager should be of correct type", typeof(HRRecruitmentJobCampaignDocManagerInfo), campaign.DocManagerInfo.GetType());
		}

		public void TestHV_OH_ClientAccount()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress mainAddress = org.MainAddress;
			OrgAddress otherAddressOnOrg = org.Addresses.AddNew();
			OrgAddress otherAddress = Factory.New<OrgAddress>();

			OrgContact contact = org.Contacts.AddNew();
			OrgContact otherContact = Factory.New<OrgContact>();

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			Assert("Client Address should be readonly", campaign.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should be readonly", campaign.HV_OC_ClientContactInfo.ReadOnly);
			campaign.HV_OH_ClientAccount = org.PK;

			Assert("Client Address should NOT be readonly", !campaign.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should NOT be readonly", !campaign.HV_OC_ClientContactInfo.ReadOnly);

			AssertEquals("Client Address should be defaulted to main address of Org", mainAddress.PK, campaign.HV_OA_ClientAddress);
			AssertCollectionContains("Other Address on Org should be in Campaign orgaddress lookups", otherAddressOnOrg, campaign.Lookups.ClientAddresses);
			AssertCollectionContains("Org Contact should be in Campaign orgcontact lookups", contact, campaign.Lookups.ClientContacts);

			AssertCollectionNotContains("Other Address should NOT be in Campaign orgaddress lookups", otherAddress, campaign.Lookups.ClientAddresses);
			AssertCollectionNotContains("Other Contact should NOT be in Campaign orgcontact lookups", otherContact, campaign.Lookups.ClientContacts);

			campaign.HV_OC_ClientContact = contact.PK;

			campaign.HV_OH_ClientAccount = ZGuid.Invalid;
			Assert("Client Address should be readonly", campaign.HV_OA_ClientAddressInfo.ReadOnly);
			Assert("Client Contact should be readonly", campaign.HV_OC_ClientContactInfo.ReadOnly);
			AssertEquals("Client Address should be empty", ZGuid.Empty, campaign.HV_OA_ClientAddress);
			AssertEquals("Client Contact should be empty", ZGuid.Empty, campaign.HV_OC_ClientContact);
			AssertEquals("Contact lookups on campaign should be empty", 0, campaign.Lookups.ClientContacts.Count);
			AssertEquals("Address lookups on campaign should be empty", 0, campaign.Lookups.ClientAddresses.Count);
		}

		public void TestSalaryRangeAsText()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_WageLow = 30000;
			campaign.HV_WageHigh = 50000;
			campaign.HV_RX_NKWageRangeCurrency = "AUD";
			AssertEquals("AUD 30,000 - 50,000", campaign.SalaryRangeAsText);

			campaign.HV_WageHigh = 30000;
			AssertEquals("AUD 30,000", campaign.SalaryRangeAsText);

			campaign.HV_RX_NKWageRangeCurrency = "HKD";
			AssertEquals("HKD 30,000", campaign.SalaryRangeAsText);

			campaign.HV_RX_NKWageRangeCurrency = "IDR";
			campaign.HV_WageLow = 700000;
			campaign.HV_WageHigh = 2000000;
			AssertEquals("IDR 700,000 - 2,000,000", campaign.SalaryRangeAsText);
		}

		public void TestEffectiveEndDateAsText()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_CampaignEndDate = new ZDate(2008, 2, 1);
			AssertEquals("01-Feb-08", campaign.EffectiveEndDateAsText);

			campaign.HV_CampaignEndDate = new ZDate(2009, 12, 12);
			AssertEquals("12-Dec-09", campaign.EffectiveEndDateAsText);

			campaign.HV_CampaignEndDate = ZDate.Empty;
			AssertEquals("Ongoing", campaign.EffectiveEndDateAsText);
		}

		public void TestNewApplicantsAdded()
		{
			IEnumerable<HRJobApplication> newApplications = null;
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign.NewApplicationsAdded += (sender, args) => newApplications = args.applications;
			Factory.Save();
			AssertNull("Event should not be called", newApplications);

			HRJobApplication newButCancelledApp = campaign.Applications.AddNew();
			newButCancelledApp.Delete();
			Factory.Save();
			AssertNull("Event should not be called", newApplications);

			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";
			HRJobApplication newApp = campaign.Applications.AddNew();
			newApp.HP_HA = applicant.PK;
			Factory.Save();
			AssertNotNull(newApplications);
			Assert(newApplications.Contains(newApp));
		}

		public void TestCampaignDateUNLOCO()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var client = Factory.New<OrgHeader>();
			campaign.HV_OH_ClientAccount = client.PK;

			using (Env.SetTemporaryUserContext(string.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNoExceptionThrown(() => { var unloco = campaign.CampaignDateUNLOCO; });
			}
		}

		public void TestCampaignID()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_AdTitle = "Software Developer";
			AssertEquals("Software Developer", campaign.CampaignID);

			campaign.HV_CampaignStartDate = new ZDateTime(2011, 6, 6);
			AssertEquals("110606_Software Developer", campaign.CampaignID);

			campaign.HV_CampaignStartDate = ZDateTime.Empty;
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			campaign.HV_OA_ClientAddress = mainAddress.PK;
			AssertEquals("AUSYD_Software Developer", campaign.CampaignID);

			campaign.HV_CampaignStartDate = new ZDateTime(2011, 6, 6);
			AssertEquals("110606_AUSYD_Software Developer", campaign.CampaignID);

			campaign.HV_AdTitle = "Software Developer_(Sydney)";
			AssertEquals("110606_AUSYD_Software Developer_(Sydney)", campaign.CampaignID);
		}

		[ExpectNoExceptions]
		public void TestGetInfoFromCampaignID()
		{
			ZDateTime campaignStartDate;
			ZString campaignUNLOCO;
			ZString adTitle;

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals(ZString.Empty, adTitle);
			AssertEquals(ZDate.Empty, campaignStartDate);
			AssertEquals(ZString.Empty, campaignUNLOCO);

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("Software Developer", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals("Software Developer", adTitle);
			AssertEquals(ZDate.Empty, campaignStartDate);
			AssertEquals(ZString.Empty, campaignUNLOCO);

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("110606_Software Developer", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals("Software Developer", adTitle);
			AssertEquals(new ZDateTime(2011, 6, 6), campaignStartDate);
			AssertEquals(ZString.Empty, campaignUNLOCO);

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("AUSYD_Software Developer", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals("Software Developer", adTitle);
			AssertEquals(ZDate.Empty, campaignStartDate);
			AssertEquals("AUSYD", campaignUNLOCO);

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("110606_AUSYD_Software Developer", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals("Software Developer", adTitle);
			AssertEquals(new ZDateTime(2011, 6, 6), campaignStartDate);
			AssertEquals("AUSYD", campaignUNLOCO);

			HRRecruitmentJobCampaign.GetInfoFromCampaignID("110606_AUSYD_Software Developer_(Sydney)", out adTitle, out campaignStartDate, out campaignUNLOCO);
			AssertEquals("Software Developer_(Sydney)", adTitle);
			AssertEquals(new ZDateTime(2011, 6, 6), campaignStartDate);
			AssertEquals("AUSYD", campaignUNLOCO);
		}

		public void TestHV_CampaignStartAndHV_CampaignEndDateInUTC()
		{
			RefTimeZone timeZone = Factory.New<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "DDT";
			timeZone.R2_OffsetMinutesFromUTC = 120;

			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;
			timeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet.R3_TimeZoneSetName = "DD";

			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "DDVVV";
			port.RL_R3 = timeZoneSet.PK;
			port.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_RL_NKHomePort = port.Code;

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";

			GlbStaff createUser = Factory.NewWithValidTestData<GlbStaff>();
			createUser.GS_Code = "AAA";
			createUser.GS_LoginName = "AAA User";
			createUser.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRRecruitmentJobCampaign campaign = new BusinessObjectFactory().NewWithValidTestData<HRRecruitmentJobCampaign>();

				campaign.HV_CampaignStartDateLocal = new ZDateTime(2012, 3, 19);
				AssertEquals(campaign.HV_CampaignStartDateLocal.AddHours(-2), campaign.HV_CampaignStartDate);
				AssertEquals(campaign.HV_CampaignStartDateLocal.AddHours(-2), campaign.HV_CampaignStartDate);
				AssertEquals(string.Concat(port.RL_RN_NKCountryCode, " ", timeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);

				campaign.HV_CampaignStartDateLocal = new ZDateTime(2012, 4, 2);
				campaign.Factory.Save();
				AssertEquals(campaign.HV_CampaignStartDateLocal.AddHours(-2), campaign.HV_CampaignStartDate);

				campaign.HV_CampaignEndDateLocal = new ZDateTime(2013, 2, 14);
				AssertEquals(campaign.HV_CampaignEndDateLocal.AddHours(-2), campaign.HV_CampaignEndDate);

				campaign.HV_CampaignEndDateLocal = new ZDateTime(2013, 5, 2);
				campaign.Factory.Save();
				AssertEquals(campaign.HV_CampaignEndDateLocal.AddHours(-2), campaign.HV_CampaignEndDate);

				AssertEquals(new ZDateTime(2012, 4, 1, 22, 0, 0), campaign.HV_CampaignStartDate);
				AssertEquals(new ZDateTime(2013, 5, 1, 22, 0, 0), campaign.HV_CampaignEndDate);
				campaign.Factory.Save();
				AssertEquals(string.Concat(port.RL_RN_NKCountryCode, " ", timeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);

				campaign.HV_CampaignStartDateLocal = new ZDateTime(2012, 4, 5);
				campaign.Factory.Save();
				AssertEquals(new ZDateTime(2012, 4, 4, 22, 0, 0), campaign.HV_CampaignStartDate);

				campaign.HV_CampaignEndDateLocal = new ZDateTime(2013, 5, 2);
				campaign.Factory.Save();
				AssertEquals(new ZDateTime(2013, 5, 1, 22, 0, 0), campaign.HV_CampaignEndDate);
			}
		}

		public void TestHV_CampaignStartAndHV_CampaignEndDateTimeZoneCalculation()
		{
			RefTimeZone branchTimeZone = Factory.New<RefTimeZone>();
			branchTimeZone.R2_CivilianTimeZoneCode = "CTA";
			branchTimeZone.R2_OffsetMinutesFromUTC = 600;

			RefTimeZoneSet branchTimeZoneSet = Factory.New<RefTimeZoneSet>();
			branchTimeZoneSet.R3_R2_StandardZone = branchTimeZone.PK;
			branchTimeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			branchTimeZoneSet.R3_TimeZoneSetName = "AZ";

			RefUNLOCO branchPort = Factory.New<RefUNLOCO>();
			branchPort.RL_Code = "AAVVV";
			branchPort.RL_R3 = branchTimeZoneSet.PK;
			branchPort.RL_RN_NKCountryCode = "AA";

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_RL_NKHomePort = branchPort.Code;

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";

			GlbStaff createUser = Factory.NewWithValidTestData<GlbStaff>();
			createUser.GS_Code = "AAA";
			createUser.GS_LoginName = "AAA User";
			createUser.GS_GB_HomeBranch = branch.PK;

			RefTimeZone clientAddressTimeZone = Factory.New<RefTimeZone>();
			clientAddressTimeZone.R2_CivilianTimeZoneCode = "CTB";
			clientAddressTimeZone.R2_OffsetMinutesFromUTC = 120;

			RefTimeZoneSet clientAddressTimeZoneSet = Factory.New<RefTimeZoneSet>();
			clientAddressTimeZoneSet.R3_R2_StandardZone = clientAddressTimeZone.PK;
			clientAddressTimeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			clientAddressTimeZoneSet.R3_TimeZoneSetName = "BZ";

			RefUNLOCO clientAddressPort = Factory.New<RefUNLOCO>();
			clientAddressPort.RL_Code = "BBVVV";
			clientAddressPort.RL_R3 = clientAddressTimeZoneSet.PK;
			clientAddressPort.RL_RN_NKCountryCode = "BB";

			RefTimeZone orgMainAddressTimeZone = Factory.New<RefTimeZone>();
			orgMainAddressTimeZone.R2_CivilianTimeZoneCode = "CTC";
			orgMainAddressTimeZone.R2_OffsetMinutesFromUTC = 540;

			RefTimeZoneSet orgMainAddressTimeZoneSet = Factory.New<RefTimeZoneSet>();
			orgMainAddressTimeZoneSet.R3_R2_StandardZone = orgMainAddressTimeZone.PK;
			orgMainAddressTimeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			orgMainAddressTimeZoneSet.R3_TimeZoneSetName = "CZ";

			RefUNLOCO orgMainAddressPort = Factory.New<RefUNLOCO>();
			orgMainAddressPort.RL_Code = "CCVVV";
			orgMainAddressPort.RL_R3 = orgMainAddressTimeZoneSet.PK;
			orgMainAddressPort.RL_RN_NKCountryCode = "CC";
			Factory.Save();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress clientAddress = Factory.NewWithValidTestData<OrgAddress>();
			clientAddress.OA_RL_NKRelatedPortCode = clientAddressPort.RL_Code;

			OrgAddress orgMainAddress = Factory.Load<OrgAddress>(org1.MainAddress.PK);
			orgMainAddress.OA_RL_NKRelatedPortCode = orgMainAddressPort.RL_Code;
			Factory.Save();

			HRRecruitmentJobCampaign hrCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			hrCampaign.HV_OH_ClientAccount = ZGuid.Empty;
			hrCampaign.HV_OA_ClientAddress = ZGuid.Empty;
			hrCampaign.HV_CampaignStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			hrCampaign.HV_CampaignEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRRecruitmentJobCampaign campaign = new BusinessObjectFactory().Load<HRRecruitmentJobCampaign>(hrCampaign.PK);
				AssertEquals(campaign.HV_CampaignStartDate.AddHours(10), campaign.HV_CampaignStartDateLocal);
				AssertEquals(campaign.HV_CampaignEndDate.AddHours(10), campaign.HV_CampaignEndDateLocal);
				AssertEquals(string.Concat(branchPort.RL_RN_NKCountryCode, " ", branchTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);

				campaign.HV_OH_ClientAccount = org1.PK;
				campaign.HV_OA_ClientAddress = clientAddress.PK;
				AssertEquals(campaign.HV_CampaignStartDateLocal.Date, new ZDate(2013, 5, 5));
				AssertEquals(campaign.HV_CampaignEndDateLocal.Date, new ZDate(2013, 6, 18));

				AssertEquals(campaign.HV_CampaignStartDate, new ZDateTime(2013, 5, 4, 22, 0, 0));
				AssertEquals(campaign.HV_CampaignEndDate, new ZDateTime(2013, 6, 17, 22, 0, 0));

				AssertEquals(string.Concat(clientAddressPort.RL_RN_NKCountryCode, " ", clientAddressTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);
			}

			hrCampaign.HV_OH_ClientAccount = org1.PK;
			hrCampaign.HV_OA_ClientAddress = clientAddress.PK;
			hrCampaign.HV_CampaignStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			hrCampaign.HV_CampaignEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRRecruitmentJobCampaign campaign = new BusinessObjectFactory().Load<HRRecruitmentJobCampaign>(hrCampaign.PK);
				AssertEquals(campaign.HV_CampaignStartDate.AddHours(2), campaign.HV_CampaignStartDateLocal);
				AssertEquals(campaign.HV_CampaignEndDate.AddHours(2), campaign.HV_CampaignEndDateLocal);
				AssertEquals(string.Concat(clientAddressPort.RL_RN_NKCountryCode, " ", clientAddressTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);

				campaign.HV_OA_ClientAddress = Guid.Empty;
				AssertEquals(campaign.HV_CampaignStartDateLocal.Date, new ZDate(2013, 5, 4));
				AssertEquals(campaign.HV_CampaignEndDateLocal.Date, new ZDate(2013, 6, 17));

				AssertEquals(campaign.HV_CampaignStartDate, new ZDateTime(2013, 5, 3, 15, 0, 0));
				AssertEquals(campaign.HV_CampaignEndDate, new ZDateTime(2013, 6, 16, 15, 0, 0));
				AssertEquals(string.Concat(orgMainAddressPort.RL_RN_NKCountryCode, " ", orgMainAddressTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);
			}

			hrCampaign.HV_OH_ClientAccount = org1.PK;
			hrCampaign.HV_OA_ClientAddress = Guid.Empty;
			hrCampaign.HV_CampaignStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			hrCampaign.HV_CampaignEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRRecruitmentJobCampaign campaign = new BusinessObjectFactory().Load<HRRecruitmentJobCampaign>(hrCampaign.PK);

				AssertEquals(campaign.HV_CampaignStartDate.AddHours(9), campaign.HV_CampaignStartDateLocal);
				AssertEquals(campaign.HV_CampaignEndDate.AddHours(9), campaign.HV_CampaignEndDateLocal);
				AssertEquals(string.Concat(orgMainAddressPort.RL_RN_NKCountryCode, " ", orgMainAddressTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);

				campaign.HV_OH_ClientAccount = Guid.Empty;
				AssertEquals(campaign.HV_CampaignStartDateLocal.Date, new ZDate(2013, 5, 5));
				AssertEquals(campaign.HV_CampaignEndDateLocal.Date, new ZDate(2013, 6, 18));

				AssertEquals(campaign.HV_CampaignStartDate, new ZDateTime(2013, 5, 4, 14, 0, 0));
				AssertEquals(campaign.HV_CampaignEndDate, new ZDateTime(2013, 6, 17, 14, 0, 0));
				AssertEquals(string.Concat(branchPort.RL_RN_NKCountryCode, " ", branchTimeZone.R2_CivilianTimeZoneCode), campaign.CampaignStartDateTimeZone);
			}
		}
	}
}
