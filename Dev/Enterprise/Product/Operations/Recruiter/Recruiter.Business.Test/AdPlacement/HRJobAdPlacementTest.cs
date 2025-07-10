using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobAdPlacement))]
	sealed class HRJobAdPlacementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCampaign()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobAdPlacement ad = campaign.AdPlacements.AddNew();

			AssertEquals("Ad Campaign should match", campaign, ad.Campaign);
		}

		public void TestIsAutoLogged()
		{
			HRJobAdPlacement placement = Factory.NewWithValidTestData<HRJobAdPlacement>();
			Factory.Save();

			AssertEquals("One log should exist", 1, placement.Logs.GetAllLogs().Count);
		}

		public void TestFillWithValidTestDataCore()
		{
			HRJobAdPlacement placement = Factory.NewWithValidTestData<HRJobAdPlacement>();
			Assert("Placement should be for a valid Camapaign", placement.HQ_HV.IsValid);
		}

		public void TestHQ_EffectiveStartAndHQ_EffectiveEndDateInUTC()
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

			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign.HV_OH_ClientAccount = ZGuid.Empty;
			campaign.HV_OC_ClientContact = ZGuid.Empty;
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRJobAdPlacement placement = new BusinessObjectFactory().NewWithValidTestData<HRJobAdPlacement>();
				placement.HQ_HV = campaign.PK;

				placement.HQ_EffectiveStartDateLocal = new ZDateTime(2012, 3, 19);
				AssertEquals(placement.HQ_EffectiveStartDateLocal.AddHours(-2), placement.HQ_EffectiveStartDate);
				AssertEquals(placement.HQ_EffectiveStartDateLocal.AddHours(-2), placement.HQ_EffectiveStartDate);

				placement.HQ_EffectiveStartDateLocal = new ZDateTime(2012, 4, 2);
				placement.Factory.Save();
				AssertEquals(placement.HQ_EffectiveStartDateLocal.AddHours(-2), placement.HQ_EffectiveStartDate);

				placement.HQ_EffectiveEndDateLocal = new ZDateTime(2013, 2, 14);
				AssertEquals(placement.HQ_EffectiveEndDateLocal.AddHours(-2), placement.HQ_EffectiveEndDate);

				placement.HQ_EffectiveEndDateLocal = new ZDateTime(2013, 5, 2);
				placement.Factory.Save();
				AssertEquals(placement.HQ_EffectiveEndDateLocal.AddHours(-2), placement.HQ_EffectiveEndDate);

				AssertEquals(new ZDateTime(2012, 4, 1, 22, 0, 0), placement.HQ_EffectiveStartDate);
				AssertEquals(new ZDateTime(2013, 5, 1, 22, 0, 0), placement.HQ_EffectiveEndDate);
				placement.Factory.Save();

				placement.HQ_EffectiveStartDateLocal = new ZDateTime(2012, 4, 5);
				placement.Factory.Save();
				AssertEquals(new ZDateTime(2012, 4, 4, 22, 0, 0), placement.HQ_EffectiveStartDate);

				placement.HQ_EffectiveEndDateLocal = new ZDateTime(2013, 5, 2);
				placement.Factory.Save();
				AssertEquals(new ZDateTime(2013, 5, 1, 22, 0, 0), placement.HQ_EffectiveEndDate);
			}
		}

		public void TestHQ_EffectiveStartAndHQ_EffectiveEndDateTimeZoneCalculation()
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

			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign.HV_OH_ClientAccount = ZGuid.Empty;
			campaign.HV_OA_ClientAddress = ZGuid.Empty;

			HRJobAdPlacement jobPlacement = Factory.NewWithValidTestData<HRJobAdPlacement>();
			jobPlacement.HQ_EffectiveStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			jobPlacement.HQ_EffectiveEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			jobPlacement.HQ_HV = campaign.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRJobAdPlacement placement = new BusinessObjectFactory().Load<HRJobAdPlacement>(jobPlacement.PK);
				AssertEquals(placement.HQ_EffectiveStartDate.AddHours(10), placement.HQ_EffectiveStartDateLocal);
				AssertEquals(placement.HQ_EffectiveEndDate.AddHours(10), placement.HQ_EffectiveEndDateLocal);

				placement.Campaign.HV_OH_ClientAccount = org1.PK;
				placement.Campaign.HV_OA_ClientAddress = clientAddress.PK;

				AssertEquals(placement.HQ_EffectiveStartDateLocal.Date, new ZDate(2013, 5, 5));
				AssertEquals(placement.HQ_EffectiveEndDateLocal.Date, new ZDate(2013, 6, 18));
				AssertEquals(placement.HQ_EffectiveStartDate, new ZDateTime(2013, 5, 4, 22, 0, 0));
				AssertEquals(placement.HQ_EffectiveEndDate, new ZDateTime(2013, 6, 17, 22, 0, 0));
			}

			jobPlacement.HQ_EffectiveStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			jobPlacement.HQ_EffectiveEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			campaign.HV_OH_ClientAccount = org1.PK;
			campaign.HV_OA_ClientAddress = clientAddress.PK;
			jobPlacement.HQ_HV = campaign.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRJobAdPlacement placement = new BusinessObjectFactory().Load<HRJobAdPlacement>(jobPlacement.PK);
				AssertEquals(placement.HQ_EffectiveStartDate.AddHours(2), placement.HQ_EffectiveStartDateLocal);
				AssertEquals(placement.HQ_EffectiveEndDate.AddHours(2), placement.HQ_EffectiveEndDateLocal);

				placement.Campaign.HV_OA_ClientAddress = ZGuid.Empty;
				AssertEquals(placement.HQ_EffectiveStartDateLocal.Date, new ZDate(2013, 5, 4));
				AssertEquals(placement.HQ_EffectiveEndDateLocal.Date, new ZDate(2013, 6, 17));

				AssertEquals(placement.HQ_EffectiveStartDate, new ZDateTime(2013, 5, 3, 15, 0, 0));
				AssertEquals(placement.HQ_EffectiveEndDate, new ZDateTime(2013, 6, 16, 15, 0, 0));
			}

			jobPlacement.HQ_EffectiveStartDate = new ZDateTime(2013, 5, 4, 16, 0, 0);
			jobPlacement.HQ_EffectiveEndDate = new ZDateTime(2013, 6, 17, 16, 0, 0);
			campaign.HV_OH_ClientAccount = org1.PK;
			campaign.HV_OA_ClientAddress = ZGuid.Empty;
			jobPlacement.HQ_HV = campaign.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				HRJobAdPlacement placement = new BusinessObjectFactory().Load<HRJobAdPlacement>(jobPlacement.PK);
				AssertEquals(placement.HQ_EffectiveStartDate.AddHours(9), placement.HQ_EffectiveStartDateLocal);
				AssertEquals(placement.HQ_EffectiveEndDate.AddHours(9), placement.HQ_EffectiveEndDateLocal);

				placement.Campaign.HV_OH_ClientAccount = ZGuid.Empty;
				AssertEquals(placement.HQ_EffectiveStartDateLocal.Date, new ZDate(2013, 5, 5));
				AssertEquals(placement.HQ_EffectiveEndDateLocal.Date, new ZDate(2013, 6, 18));

				AssertEquals(placement.HQ_EffectiveStartDate, new ZDateTime(2013, 5, 4, 14, 0, 0));
				AssertEquals(placement.HQ_EffectiveEndDate, new ZDateTime(2013, 6, 17, 14, 0, 0));
			}
		}
	}
}
