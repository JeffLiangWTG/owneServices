using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgLandedCostingPrefsCollection))]
	sealed class OrgLandedCostingPrefsCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Group Retrieval

		public void TestGetLandedCostingGroupNameFromID()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertEquals("No groupName with ID 1", "", org.LandedCostingPreferences.GetLandedCostingGroupNameFromID(1));

			OrgLandedCostingPrefs pref = org.LandedCostingPreferences.AddNew();
			pref.O9_LandedCostGroup = 1;
			pref.O9_LandedCostGroupName = "Freight";
			AssertEquals("GroupName with ID 1", pref.O9_LandedCostGroupName, org.LandedCostingPreferences.GetLandedCostingGroupNameFromID(1));
		}

		public void TestGetLandedCostingDistributionCodeFromID()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertEquals("No Group Cost Distribution Code with ID 1", "", org.LandedCostingPreferences.GetLandedCostingGroupCostDistributeByFromID(1));

			OrgLandedCostingPrefs pref = org.LandedCostingPreferences.AddNew();
			pref.O9_LandedCostGroup = 1;
			pref.O9_LandedCostGroupName = "Freight";
			pref.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;
			AssertEquals("Group Cost Distribution Code with ID 1", pref.O9_DistributeCostBy, org.LandedCostingPreferences.GetLandedCostingGroupCostDistributeByFromID(1));
		}

		public void TestGetLandedCostingGroupFromID()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertNull("No preference with ID 1", org.LandedCostingPreferences.GetLandedCostingGroupFromID(1));

			OrgLandedCostingPrefs pref = org.LandedCostingPreferences.AddNew();
			AssertNull("No preference with ID 1", org.LandedCostingPreferences.GetLandedCostingGroupFromID(1));

			pref.O9_LandedCostGroup = 1;
			AssertEquals("preference with ID 1", pref, org.LandedCostingPreferences.GetLandedCostingGroupFromID(1));
		}

		public void TestGetLandedCostingGroupFromChargeCode()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs pref1 = org.LandedCostingPreferences.AddNew();
			pref1.O9_LandedCostGroupName = "Group 1";
			OrgLandedCostingPrefCharges pref1Charge1 = pref1.Charges.AddNew();
			pref1Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgLandedCostingPrefs pref2 = org.LandedCostingPreferences.AddNew();
			pref2.O9_LandedCostGroupName = "Group 2";
			OrgLandedCostingPrefCharges pref2Charge1 = pref2.Charges.AddNew();
			pref2Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Origin;

			OrgLandedCostingPrefs result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT);
			AssertEquals("First group returned", pref1, result);

			pref1Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Destination;

			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT);
			AssertNull("No group returned", result);

			pref2Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT);
			AssertEquals("Second group returned", pref2, result);

			pref2Charge1.O0_AC_ChargeCode = fRT.PK;
			pref2Charge1.O0_Excluded = true;
			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT);
			AssertNull("No group returned as FRT was excluded from Group 2", result);

			pref2Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Origin;
			pref2Charge1.O0_Excluded = false;
			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT);
			AssertEquals("Second group returned", pref2, result);

			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(fRT, pref2);
			AssertNull("No group returned as Pref2 was excluded", result);
		}

		public void TestGetLandedCostingGroupFromChargeGroup()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs pref1 = org.LandedCostingPreferences.AddNew();
			pref1.O9_LandedCostGroupName = "Group 1";
			OrgLandedCostingPrefCharges pref1Charge1 = pref1.Charges.AddNew();
			pref1Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgLandedCostingPrefs pref2 = org.LandedCostingPreferences.AddNew();
			pref2.O9_LandedCostGroupName = "Group 2";
			OrgLandedCostingPrefCharges pref2Charge1 = pref2.Charges.AddNew();
			pref2Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Origin;

			OrgLandedCostingPrefs result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeGroup(IncoTermChargeCodeGroupList.Codes.Freight);
			AssertEquals("First group returned", pref1, result);

			pref1Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Destination;

			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeGroup(IncoTermChargeCodeGroupList.Codes.Freight);
			AssertNull("No group returned", result);

			pref2Charge1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeGroup(IncoTermChargeCodeGroupList.Codes.Freight);
			AssertEquals("Second group returned", pref2, result);

			result = org.LandedCostingPreferences.GetLandedCostingGroupFromChargeGroup(IncoTermChargeCodeGroupList.Codes.Freight, pref2);
			AssertNull("No group returned as Pref2 was excluded", result);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = OrgHeader.New(Factory);
			return org.LandedCostingPreferences;
		}

		#endregion
	}
}
