using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LCPreferenceFallBackCalculatorTest : TestCaseWithFactory
	{
		public void TestCachedRegistryRefreshed()
		{
			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "ONE";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(null);
			AssertEquals("One preference", 1, calculator.GetPreferences().Length);

			ILandedCostPreference pref = calculator.GetPreferences()[0];
			AssertEquals("Group ID", (ZByte)1, pref.LCGroupID);
			AssertEquals("Group Name", "ONE", pref.LCGroupName);
			AssertEquals("Group Distribution", landedCostingGroup.CostDistributionCode, pref.DistributionBy);

			LandedCostingGroup landedCostingGroup2 = collection.AddNew();
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup2.GroupName = "TWO";
			landedCostingGroup2.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("One preference", 2, calculator.GetPreferences().Length);

			ILandedCostPreference pref2 = calculator.GetPreferences()[1];
			AssertEquals("Group ID", (ZByte)2, pref2.LCGroupID);
			AssertEquals("Group Name", "TWO", pref2.LCGroupName);
			AssertEquals("Group Distribution", landedCostingGroup.CostDistributionCode, pref2.DistributionBy);
		}

		public void TestGetPreferenceFromChargeCodeWhenConsigneeHasOne()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			OrgLandedCostingPrefs preference = Consignee.LandedCostingPreferences.AddNew();
			preference.O9_LandedCostGroupName = "TEST";
			preference.O9_LandedCostGroup = 10;

			OrgLandedCostingPrefCharges consigneeChargeRow = preference.Charges.AddNew();
			consigneeChargeRow.O0_AC_ChargeCode = chargeCode.PK;

			ChargeGroupAndChargeCode registryChargeRow = FreightDataRegistry.Instance.LandedCostingPreferences.Value[0].Charges.AddNew();
			registryChargeRow.ChargeCodePK = chargeCode.PK;

			AssertEquals("Consignee has a preference", 1, Consignee.LandedCostingPreferences.Count);
			AssertEquals("Preference should come from Consignee", "TEST", calculator.GetPreferenceFromAccChargeCode(chargeCode).LCGroupName);
		}

		public void TestGetPreferenceFromChargeCodeWhenConsigneeHasNone()
		{
			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			AssertEquals("Consignee doesn't have any preferences", 0, Consignee.LandedCostingPreferences.Count);

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup group = collection.AddNew();
			group.GroupID = 3;
			group.GroupName = "Not TEST";
			group.CostDistributionCode = CostDistributionMechanismList.Codes.Actual;
			ChargeGroupAndChargeCode charge = group.Charges.AddNew();
			charge.ChargeCodePK = chargeCode.PK;
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("LC Group for New Charge", (ZByte)3, calculator.GetPreferenceFromAccChargeCode(chargeCode).LCGroupID);
		}

		public void TestGetPreferencesWhenConsigneeHasOne()
		{
			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			OrgLandedCostingPrefs preference = Consignee.LandedCostingPreferences.AddNew();
			preference.O9_LandedCostGroupName = "TEST";
			preference.O9_LandedCostGroup = 10;

			Assert("Registry has a preference, too", FreightDataRegistry.Instance.LandedCostingPreferences.Value.Count > 0);
			Assert("Preference list should come from Consignee", calculator.GetPreferences().Length == 1);
			AssertEquals("Preference list should come from Consignee", (ZByte)10, calculator.GetPreferences()[0].LCGroupID);
			AssertEquals("Preference list should come from Consignee", "TEST", calculator.GetPreferences()[0].LCGroupName);
		}

		public void TestGetPreferencesWhenConsigneeHasNone()
		{
			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			AssertEquals("Consignee doesn't have any preferences", 0, Consignee.LandedCostingPreferences.Count);
			Assert("Registry has a preference", FreightDataRegistry.Instance.LandedCostingPreferences.Value.Count > 0);

			Assert("Preference list should come from Registry", calculator.GetPreferences().Length > 0);
		}

		public void TestGetGroupNameAndDistributionCodeFromIDWhenConsigneeHasOne()
		{
			OrgLandedCostingPrefs preference = Consignee.LandedCostingPreferences.AddNew();
			preference.O9_LandedCostGroupName = "TEST";
			preference.O9_LandedCostGroup = 1;
			preference.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Item;

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup group = collection.AddNew();
			group.GroupID = 1;
			group.GroupName = "Not TEST";
			group.CostDistributionCode = CostDistributionMechanismList.Codes.Actual;

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Assert("Registry has a preference with id 1", !string.IsNullOrEmpty(FreightDataRegistry.Instance.LandedCostingPreferences.Value.GetLandedCostingGroupNameFromID(1)));
			Assert("Registry has a preference with id 1", FreightDataRegistry.Instance.LandedCostingPreferences.Value.GetLandedCostingGroupNameFromID(1) != "TEST");
			AssertEquals("Registry has a preference with id 1 of cost distribution code", CostDistributionMechanismList.Codes.Actual, FreightDataRegistry.Instance.LandedCostingPreferences.Value.GetLandedCostingGroupDistributionCodeFromID(1));

			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			AssertEquals("Preference from Consignee", "TEST", calculator.GetLandedCostingGroupNameFromID(1));
			AssertEquals("Cost Distribution Code from Consignee", CostDistributionMechanismList.Codes.Item, calculator.GetLandedCostingGroupDistributionCodeFromID(1));
		}

		public void TestGetGroupNameAndDistributionCodeFromIDWhenConsigneeHasNone()
		{
			string firstRegistryGroupName = FreightDataRegistry.Instance.LandedCostingPreferences.Value[0].GroupName;
			string firstRegistryCostDistributionCode = FreightDataRegistry.Instance.LandedCostingPreferences.Value[0].CostDistributionCode;

			LCPreferenceFallBackCalculator calculator = new LCPreferenceFallBackCalculator(Consignee);
			AssertEquals("First Group Name", firstRegistryGroupName, calculator.GetLandedCostingGroupNameFromID(1));
			AssertEquals("First Group Cost Distribution Code", firstRegistryCostDistributionCode, calculator.GetLandedCostingGroupDistributionCodeFromID(1));
		}

		#region Implementation
		OrgHeader Consignee;

		protected override void SetUp()
		{
			base.SetUp();
			Consignee = Factory.New<OrgHeader>();
		}

		#endregion
	}
}
