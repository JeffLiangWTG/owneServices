using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgLandedCostingPrefsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGroupID()
		{
			Prefs.O9_LandedCostGroup = 0;
			AssertHasErrors("Group ID Mandatory", Prefs.O9_LandedCostGroupInfo);

			Prefs.O9_LandedCostGroup = 9;
			AssertNoErrors("Group ID specified", Prefs.O9_LandedCostGroupInfo);

			OrgLandedCostingPrefs prefs2 = Prefs.Header.LandedCostingPreferences.AddNew();
			prefs2.O9_LandedCostGroup = 9;
			AssertHasErrors("Same ID as previous pref - error", prefs2.O9_LandedCostGroupInfo);

			prefs2.O9_LandedCostGroup = 10;
			AssertNoErrors("Different ID as previous pref - error cleared", prefs2.O9_LandedCostGroupInfo);
		}

		public void TestDistributeCostsBy()
		{
			Prefs.O9_DistributeCostBy = "";
			AssertHasErrors("Cost Distribution Mandatory", Prefs.O9_DistributeCostByInfo);

			Prefs.O9_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;
			AssertNoErrors("Cost Distribution specified", Prefs.O9_DistributeCostByInfo);

			Prefs.O9_DistributeCostBy = "ZUB";
			AssertHasErrors("Cost Distribution invalid", Prefs.O9_DistributeCostByInfo);
		}

		public void TestGroupName()
		{
			Prefs.O9_LandedCostGroupName = "";
			AssertHasErrors("Group Name Mandatory", Prefs.O9_LandedCostGroupNameInfo);

			Prefs.O9_LandedCostGroupName = "Zubin";
			AssertNoErrors("Group Name specified", Prefs.O9_LandedCostGroupNameInfo);
		}

		#region Implementation

		OrgLandedCostingPrefs Prefs;

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader org = OrgHeader.New(Factory);
			Prefs = org.LandedCostingPreferences.AddNew();
		}

		#endregion
	}
}
