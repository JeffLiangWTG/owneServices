using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgLandedCostingPrefCharges))]
	sealed class OrgLandedCostingPrefChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLogging()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Desc1";
			OrgLandedCostingPrefCharges charges = Factory.NewWithValidTestData<OrgLandedCostingPrefCharges>();

			charges.O0_AC_ChargeCode = chargeCode.PK;

			charges.Lookups.IncoTermChargeGroups.AddPair("../", "Desc2");
			charges.O0_ChargeGroup = "../";

			Factory.Save();

			AssertEquals("Should have one log", 1, charges.Logs.GetAllLogs().Count);
			ZString expectedReference = "Charge Group / Code combination Charge Code: Desc1 Charge Group: Desc2";
			AssertEquals("Log should have reference", expectedReference, charges.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);
		}

		public void TestCompanyCode()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Name = "GC1";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "GC2";

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = company1.PK;

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_GC = company2.PK;

			var orgLandedCostingPrefCharges1 = Factory.NewWithValidTestData<OrgLandedCostingPrefCharges>();
			orgLandedCostingPrefCharges1.O0_AC_ChargeCode = chargeCode1.PK;

			var orgLandedCostingPrefCharges2 = Factory.NewWithValidTestData<OrgLandedCostingPrefCharges>();
			orgLandedCostingPrefCharges2.O0_AC_ChargeCode = chargeCode2.PK;

			var orgLandedCostingPrefCharges3 = Factory.NewWithValidTestData<OrgLandedCostingPrefCharges>();
			orgLandedCostingPrefCharges3.O0_AC_ChargeCode = ZGuid.Empty;
			Factory.Save();

			AssertEquals("GC1", company1.GC_Name, orgLandedCostingPrefCharges1.CompanyName);
			AssertEquals("GC2", company2.GC_Name, orgLandedCostingPrefCharges2.CompanyName);
			AssertEquals("Empty charge code", ZString.Empty, orgLandedCostingPrefCharges3.CompanyName);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldLandedCostingValue = Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed;

			try
			{
				OrgLandedCostingPrefs testPref = OrgInDB.LandedCostingPreferences.AddNew();
				OrgLandedCostingPrefCharges testCharge = testPref.Charges.AddNew();

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testCharge.O0_AC_ChargeCodeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCharge.O0_ChargeGroupInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testCharge.O0_ExcludedInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testCharge.O0_AC_ChargeCodeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCharge.O0_ChargeGroupInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testCharge.O0_ExcludedInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = oldLandedCostingValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			return prefs.Charges.AddNew();
		}

		#endregion
	}
}
