using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandCostInputLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParentAccociableList()
		{
			DummyLandedCostDistributeTo dummyDistribute = Factory.New<DummyLandedCostDistributeTo>();
			dummy.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { dummyDistribute };

			AssertEquals("1 associable parent", 1, costInput.Lookups.ParentAssociableList.Count);
		}

		public void TestDistributeCostBy()
		{
			var lookups = costInput.Lookups.DistributeCostBy;
			AssertEquals("5 items in Distribute Cost By", 5, lookups.Count);
			AssertEquals(Factory.GetCachedValue<CostDistributionMechanismList>(), lookups);
		}

		public void TestLandCostGroupListFromConsignee()
		{
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgLandedCostingPrefs pref1 = consignee.LandedCostingPreferences.AddNew();
			pref1.O9_LandedCostGroup = 1;
			pref1.O9_LandedCostGroupName = "TT1";

			OrgLandedCostingPrefs pref2 = consignee.LandedCostingPreferences.AddNew();
			pref2.O9_LandedCostGroup = 2;
			pref2.O9_LandedCostGroupName = "TT2";

			dummy.ConsigneeExposed = consignee;
			AssertEquals("LC ChargeGroup List", 2, costInput.Lookups.LandCostGroupList.Count);
		}

		public void TestLandCostGroupListFromRegistryIfConsigneeDoesntHavePreference()
		{
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			dummy.ConsigneeExposed = consignee;

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 2;
			landedCostingGroup.GroupName = "TEST";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Consignee doesnt have any preferences", 0, consignee.LandedCostingPreferences.Count);
			AssertEquals("LC group list should come from registry then", 1, costInput.Lookups.LandCostGroupList.Count);
			AssertEquals("Group id", "2", costInput.Lookups.LandCostGroupList[0].Code);
			AssertEquals("Group id", "TEST", costInput.Lookups.LandCostGroupList[0].Description);
		}

		LandedCostHeader header;
		LandCostInput costInput;
		DummyLandedCostHeader dummy;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<LandedCostHeader>();
			dummy = Factory.New<DummyLandedCostHeader>();
			header.LT_ParentID = dummy.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			costInput = header.CostInputs.AddNew();
		}
	}
}
