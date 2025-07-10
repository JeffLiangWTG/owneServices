using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRRecruitmentJobCampaignFindBoxListProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetBizObjFromCode()
		{
			CreateCampaigns();
			HRRecruitmentJobCampaignFindBoxListProvider listProvider = new HRRecruitmentJobCampaignFindBoxListProvider(campaigns);

			BusinessObject obj = listProvider.GetBusinessObjectFromCode("110607_Software Developer_(Sydney)");
			AssertEquals(campaign1.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("110520_AUMEL_System Administrator");
			AssertEquals(campaign2.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("AUSYD_Software Developer");
			AssertEquals(campaign3.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("110617_AUSYD_Accounting/Business Graduate");
			AssertEquals(campaign4.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("060820_ABCXX_System Tester");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCode("998877_???_GGG");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCode("__AAAAA");
			AssertNull(obj);
		}

		[ExpectNoExceptions]
		public void TestGetBizObjFromCodeWithoutFilter()
		{
			CreateCampaigns();
			HRRecruitmentJobCampaignFindBoxListProvider listProvider = new HRRecruitmentJobCampaignFindBoxListProvider(campaigns);

			BusinessObject obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("110607_Software Developer_(Sydney)");
			AssertEquals(campaign1.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("110520_AUMEL_System Administrator");
			AssertEquals(campaign2.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("AUSYD_Software Developer");
			AssertEquals(campaign3.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("110617_AUSYD_Accounting/Business Graduate");
			AssertEquals(campaign4.PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("060820_ABCXX_System Tester");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("998877_???_GGG");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("__AAAAA");
			AssertNull(obj);
		}

		#region Implementation

		HRRecruitmentJobCampaignCollection campaigns;
		HRRecruitmentJobCampaign campaign1;
		HRRecruitmentJobCampaign campaign2;
		HRRecruitmentJobCampaign campaign3;
		HRRecruitmentJobCampaign campaign4;

		void CreateCampaigns()
		{
			campaigns = new HRRecruitmentJobCampaignCollection(Factory);

			campaign1 = campaigns.AddNew();
			campaign1.HV_AdTitle = "Software Developer_(Sydney)";
			campaign1.HV_CampaignStartDate = new ZDateTime(2011, 6, 7);
			campaign1.HV_OA_ClientAddress = ZGuid.Empty;

			campaign2 = campaigns.AddNew();
			campaign2.HV_AdTitle = "System Administrator";
			campaign2.HV_CampaignStartDate = new ZDateTime(2011, 5, 20);
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress2 = org2.MainAddress;
			mainAddress2.OA_RL_NKRelatedPortCode = "AUMEL";
			campaign2.HV_OA_ClientAddress = mainAddress2.PK;

			campaign3 = campaigns.AddNew();
			campaign3.HV_AdTitle = "Software Developer";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress3 = org3.MainAddress;
			mainAddress3.OA_RL_NKRelatedPortCode = "AUSYD";
			campaign3.HV_OA_ClientAddress = mainAddress3.PK;

			campaign4 = campaigns.AddNew();
			campaign4.HV_AdTitle = "Accounting/Business Graduate";
			campaign4.HV_CampaignStartDate = new ZDateTime(2011, 6, 17, 8, 30, 0);
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress4 = org4.MainAddress;
			mainAddress4.OA_RL_NKRelatedPortCode = "AUSYD";
			campaign4.HV_OA_ClientAddress = mainAddress4.PK;

			Factory.Save();
		}

		#endregion
	}
}
