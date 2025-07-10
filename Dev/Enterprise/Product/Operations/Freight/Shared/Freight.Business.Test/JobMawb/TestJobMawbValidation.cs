using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class TestJobMawbValidation : BusinessObjectValidationTestCase
	{
		public void TestZA_AirlinePrefix()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "XXX";
			AssertHasErrors(mawb.JM_Airline3DigitPrefixInfo);

			AddMawbStockManagement("081", ZGuid.Empty, ZGuid.Empty, true, true, true);
			mawb.JM_Airline3DigitPrefix = "081";
			AssertNoErrors(mawb.JM_Airline3DigitPrefixInfo);
		}

		public void TestValidateMAWB()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_MAWB = "55555554";
			AssertHasErrors(mawb.JM_MAWBInfo);

			mawb.JM_MAWB = "55555555";
			AssertNoErrors(mawb.JM_MAWBInfo);

			mawb.JM_MAWB = "";
			AssertHasErrors(mawb.JM_MAWBInfo);
		}

		public void TestValidateServiceLevel()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader carrier = factory2.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "919").PK;
			OrgCarrierServiceLevel lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			factory2.Save();

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "919";

			mawb.JM_ServiceLevel = "XXX";
			AssertHasErrors(mawb.JM_ServiceLevelInfo);

			var mawbServiceLevel = mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().FirstOrDefault();
			mawb.JM_ServiceLevel = mawbServiceLevel.PL_Code;
			AssertNoErrors(mawb.JM_ServiceLevelInfo);

			mawb.JM_ServiceLevel = "";
			AssertHasErrors(mawb.JM_ServiceLevelInfo);
		}

		public void Test_CanAddMawb_Global_NoError()
		{
			AddMawbStockManagement("919", ZGuid.Empty, ZGuid.Empty, true, false, false);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "919";
			AssertNoErrors(mawb.JM_Airline3DigitPrefixInfo);
		}

		public void Test_CanAddMawb_Global_Error()
		{
			AddMawbStockManagement("919", ZGuid.Empty, ZGuid.Empty, false, true, false);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "919";
			AssertHasErrors("Cannot add MAWB with Airline Code '919' on global level.", mawb.JM_Airline3DigitPrefixInfo);
		}

		public void Test_CanAddMawb_Company_NoError()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, ZGuid.Empty, false, true, true);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb.JM_Airline3DigitPrefix = "919";
			AssertNoErrors(mawb.JM_Airline3DigitPrefixInfo);
		}

		public void Test_CanAddMawb_Company_Error()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, ZGuid.Empty, false, false, true);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb.JM_Airline3DigitPrefix = "919";
			AssertHasErrors($"Cannot add MAWB with Airline Code '919' for company '{GlbCompany.CurrentCompany.CompanyName}' on company level.", mawb.JM_Airline3DigitPrefixInfo);
		}

		public void Test_CanAddMawb_Branch_NoError()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, false, true, true);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_Airline3DigitPrefix = "919";
			AssertNoErrors(mawb.JM_Airline3DigitPrefixInfo);
		}

		public void Test_CanAddMawb_Branch_Error()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, false, true, false);
			var mawb = Factory.New<JobMawb>();
			mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_Airline3DigitPrefix = "919";
			AssertHasErrors(mawb.JM_Airline3DigitPrefixInfo);
		}

		void AddMawbStockManagement(string airLinePrefix, ZGuid companyId, ZGuid branchId, bool canUseGlobalStock, bool canUseCompanyStock, bool canUseBranchStock)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, airLinePrefix).PK;

			carrier.OrgAirlineMAWBStockManagementCollection.DeleteAll();
			var mawbStockMangement = carrier.OrgAirlineMAWBStockManagementCollection.AddNew();
			mawbStockMangement.OHM_GC_Company = companyId;
			mawbStockMangement.OHM_GB_Branch = branchId;
			mawbStockMangement.OHM_AllowUseGlobalStock = canUseGlobalStock;
			mawbStockMangement.OHM_AllowUseCompanyStock = canUseCompanyStock;
			mawbStockMangement.OHM_AllowUseBranchStock = canUseBranchStock;

			Factory.Save();
		}
	}
}
