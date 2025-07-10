using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgLandedCostingPrefs))]
	sealed class OrgLandedCostingPrefsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBusinessObjectsWithRelatedEvents()
		{
			OrgLandedCostingPrefs prefs = Factory.New<OrgLandedCostingPrefs>();
			AssertEquals("Business objects with required fields", 0, prefs.BusinessObjectsWithRelatedEvents.Length);
			prefs.Charges.AddNew();
			AssertEquals("Business objects with required fields", 1, prefs.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestLogging()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			prefs.O9_LandedCostGroup = 5;
			prefs.O9_LandedCostGroupName = "TEST";
			prefs.O9_DistributeCostBy = "WGT";

			Factory.Save();

			AssertEquals("Should have one log", 1, prefs.Logs.GetAllLogs().Count);
			ZString expectedReference = "Landed Costing Group Name: TEST ID: 5";
			AssertEquals("Log should have reference", expectedReference, prefs.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			prefs.O9_LandedCostGroupName = "BLAH";
			Factory.Save();

			AssertEquals("Should have one log", 2, prefs.Logs.GetAllLogs().Count);
			expectedReference = "Landed Costing Group Name: BLAH(TEST) ID: 5";
			AssertEquals("Log should have reference", expectedReference, prefs.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestILandedCostPreference()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			prefs.O9_LandedCostGroup = 5;
			prefs.O9_LandedCostGroupName = "TEST";
			prefs.O9_DistributeCostBy = "WGT";
			AssertEquals("GroupID", (ZByte)5, ((ILandedCostPreference)prefs).LCGroupID);
			AssertEquals("LCGroupName", prefs.O9_LandedCostGroupName, ((ILandedCostPreference)prefs).LCGroupName);
			AssertEquals("DistributionBy", prefs.O9_DistributeCostBy, ((ILandedCostPreference)prefs).DistributionBy);
		}

		public void TestCostDistributionDesc()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			prefs.O9_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;
			AssertEquals("Correct description", CostDistributionMechanismList.Descriptions.ActualVolume, prefs.CostDistributionDesc);
		}

		#region ReadOnly Security

		public void TestChargesCollectionIsReadOnly()
		{
			bool oldLandedCostingValue = Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed;

			OrgLandedCostingPrefs testPref = OrgInDB.LandedCostingPreferences.AddNew();

			try
			{
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testPref.Charges.ReadOnly);

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
				ResetOrgInDB();
				testPref = OrgInDB.LandedCostingPreferences.AddNew();
				Assert("Access Disallowed - ReadOnly", testPref.Charges.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = oldLandedCostingValue;
			}
		}

		public void TestReadOnlySecurity()
		{
			bool oldLandedCostingValue = Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed;

			try
			{
				OrgLandedCostingPrefs testPref = OrgInDB.LandedCostingPreferences.AddNew();

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testPref.O9_DistributeCostByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testPref.O9_LandedCostGroupInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testPref.O9_LandedCostGroupNameInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testPref.O9_DistributeCostByInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testPref.O9_LandedCostGroupInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testPref.O9_LandedCostGroupNameInfo.ReadOnly);
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
			return org.LandedCostingPreferences.AddNew();
		}

		#endregion
	}
}
