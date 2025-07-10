using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class MAWBStockManagementStrategyTest : BusinessObjectValidationTestCase
	{
		public void TestCanAddMawb()
		{
			// Branch can use its own branch stock
			TestCanAddMawbCase("919", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, true, false, true, true);

			// Branch cannot use its own branch stock
			TestCanAddMawbCase("240", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, true, false, false, false);

			// Company can use company stock
			TestCanAddMawbCase("610", GlbCompany.CurrentCompany.PK, ZGuid.Empty, true, true, false, true);

			// Company cannot use company stock
			TestCanAddMawbCase("159", GlbCompany.CurrentCompany.PK, ZGuid.Empty, true, false, false, false);

			// Can use global stock
			TestCanAddMawbCase("008", ZGuid.Empty, ZGuid.Empty, true, false, false, true);

			// Cannot use global stock
			TestCanAddMawbCase("321", ZGuid.Empty, ZGuid.Empty, false, false, true, false);
		}

		public void TestCanAddMawbCase(string airLinePrefix, ZGuid companyId, ZGuid branchId, bool canUseGlobalStock, bool canUseCompanyStock, bool canUseBranchStock, bool expectedCanAddMawb)
		{
			AddMawbStockManagement(airLinePrefix, companyId, branchId, canUseGlobalStock, canUseCompanyStock, canUseBranchStock);
			AssertEquals(expectedCanAddMawb, new MAWBStockManagementStrategy(Factory).CanAddMawb(airLinePrefix, Factory.Load<GlbCompany>(companyId), Factory.Load<GlbBranch>(branchId)));
		}

		public void TestCanNotAddGlobalMawbByDefault()
		{
			AssertEquals(false, new MAWBStockManagementStrategy(Factory).CanAddMawb("008", null, null));
		}

		public void TestCanAddGlobalMawbIfAnyCompanyCanUseGlobalStock()
		{
			AddMawbStockManagement("008", GlbCompany.CurrentCompany.PK, ZGuid.Empty, true, false, false);
			AssertEquals(true, new MAWBStockManagementStrategy(Factory).CanAddMawb("008", null, null));
		}

		public void TestCanAddGlobalMawbIfAnyBranchCanUseGlobalStock()
		{
			AddMawbStockManagement("008", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, true, false, false);
			AssertEquals(true, new MAWBStockManagementStrategy(Factory).CanAddMawb("008", null, null));
		}

		public void TestGetBranchStrategy_MatchBranch()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, false, false, true, "R");
			var strategy = new MAWBStockManagementStrategy(Factory).GetBranchStrategy(GlbBranch.CurrentBranch, "919");
			AssertEquals(false, strategy.UseGlobalStock);
			AssertEquals(false, strategy.UseCompanyStock);
			AssertEquals(true, strategy.UseBranchStock);
			AssertEquals(FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.Value, strategy.UseOtherBranchStock);
		}

		public void TestGetBranchStrategy_FallbackToCompany()
		{
			AddMawbStockManagement("919", GlbCompany.CurrentCompany.PK, ZGuid.Empty, false, true, true, "N");
			var strategy = new MAWBStockManagementStrategy(Factory).GetBranchStrategy(GlbBranch.CurrentBranch, "919");
			AssertEquals(false, strategy.UseGlobalStock);
			AssertEquals(true, strategy.UseCompanyStock);
			AssertEquals(true, strategy.UseBranchStock);
			AssertEquals(false, strategy.UseOtherBranchStock);
		}

		public void TestGetBranchStrategy_FallbackToGlobal()
		{
			AddMawbStockManagement("919", ZGuid.Empty, ZGuid.Empty, true, true, true, "Y");
			var strategy = new MAWBStockManagementStrategy(Factory).GetBranchStrategy(GlbBranch.CurrentBranch, "919");
			AssertEquals(true, strategy.UseGlobalStock);
			AssertEquals(true, strategy.UseCompanyStock);
			AssertEquals(true, strategy.UseBranchStock);
			AssertEquals(true, strategy.UseOtherBranchStock);
		}

		public void TestGetCarrierOrgByAirlinePrefix()
		{
			CreateRefAirline("157");
			var carrierAu = CreateAirCarrierOrg("157", "AU");
			var carrierNz = CreateAirCarrierOrg("157", "NZ");
			var carrierUs = CreateAirCarrierOrg("157", "US");
			var branchAu = CreateBranch("AU");
			var branchNz = CreateBranch("NZ");
			var branchUs = CreateBranch("US");
			var branchCn = CreateBranch("CN");

			var strategy = new MAWBStockManagementStrategy(Factory);

			TestCase(branchCn, carrierUs, "157");
			TestCase(branchAu, carrierAu, "157");
			TestCase(branchNz, carrierNz, "157");
			TestCase(branchUs, carrierUs, "157");
			TestCase(branchAu, null, "ABC");

			void TestCase(GlbBranch currentBranch, OrgHeader expectedCarrierOrg, string airlinePrefix)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, currentBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var carrierOrg = strategy.FindCarrierBy3CharAirlineCode(airlinePrefix);
					AssertEquals(expectedCarrierOrg, carrierOrg);
				}
			}
		}

		GlbBranch CreateBranch(string countryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RN_NKCountryCode = countryCode;
			Factory.Save();

			return branch;
		}

		void CreateRefAirline(string airlinePrefix)
		{
			var refAirline = RefAirline.LoadFromAirlinePrefix(Factory, airlinePrefix);
			if (refAirline == null)
			{
				refAirline = Factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_AirlinePrefix = airlinePrefix;
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = airlinePrefix;
			}
			Factory.Save();
		}

		OrgHeader CreateAirCarrierOrg(string airlinePrefix, string countryCode)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, airlinePrefix).PK;
			carrier.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = countryCode;
			Factory.Save();

			return carrier;
		}

		void AddMawbStockManagement(string airLinePrefix, ZGuid companyId, ZGuid branchId, bool canUseGlobalStock, bool canUseCompanyStock, bool canUseBranchStock, string useOtherBranchStock = "R")
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
			mawbStockMangement.OHM_AllowUseOtherBranchStock = useOtherBranchStock;

			Factory.Save();
		}
	}
}
