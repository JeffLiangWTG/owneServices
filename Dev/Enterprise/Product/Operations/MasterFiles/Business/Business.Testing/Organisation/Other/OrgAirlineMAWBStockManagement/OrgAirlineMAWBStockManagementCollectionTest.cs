using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAirlineMAWBStockManagementCollection))]
	sealed class OrgAirlineMAWBStockManagementCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgAirlineMAWBStockManagementCollection>
	{
		#region TestGetMawbStockManagementForCurrentBranch

		public void TestGetMAWBStockManagementForCurrentBranch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();

			var stockManagement1 = CreateStockManagement(org1, GlbBranch.CurrentBranch, 60);
			var stockManagement2 = CreateStockManagement(org1, branch2, 50);
			var stockManagement3 = CreateStockManagement(org1, null, 60);
			var stockManagement4 = CreateStockManagement(org2, GlbBranch.CurrentBranch, 60);
			var stockManagement5 = CreateStockManagement(org2, branch2, 50);
			var stockManagement6 = CreateStockManagement(org2, null, 60);

			Factory.Save();

			AssertEquals("Current Branch has a match for org 1", stockManagement1, org1.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
			AssertEquals("Current Branch has a match for org 2", stockManagement4, org2.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Branch2 has a match, org1", stockManagement2, org1.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
				AssertEquals("Branch2 has a match, org2", stockManagement5, org2.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("non-listed department gets value from entry with no department, org1", stockManagement3, org1.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
				AssertEquals("non-listed department gets value from entry with no department, org2", stockManagement6, org2.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
			}

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			AssertNull("Org that has no stockManagements shouldnt find one", org3.OrgAirlineMAWBStockManagementCollection.GetMAWBStockManagementForCurrentBranch());
		}

		OrgAirlineMAWBStockManagement CreateStockManagement(OrgHeader org, GlbBranch branch, ZShort threshold)
		{
			var stockManagement = org.OrgAirlineMAWBStockManagementCollection.AddNew();
			stockManagement.OHM_GB_Branch = branch?.PK ?? ZGuid.Empty;
			stockManagement.OHM_MAWBStockThreshold = threshold;
			if (stockManagement.OHM_GB_Branch != ZGuid.Empty)
			{
				stockManagement.OHM_GC_Company = branch.Company.PK;
			}
			return stockManagement;
		}

		#endregion

		#region TestSetDefaultsForNewChild()

		public void TestSetDefaultsForNewChild()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsAirLine = true;

			var collection = org.OrgAirlineMAWBStockManagementCollection;
			var newItem = collection.AddNew();

			AssertEquals(org.PK, newItem.OHM_OH_Carrier);
		}

		#endregion

		#region Implementation

		protected override OrgAirlineMAWBStockManagementCollection GetCollectionToTest()
		{
			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "TESTORG";

			return new OrgAirlineMAWBStockManagementCollection(branchOrgProxy.Factory, branchOrgProxy);
		}

		#endregion
	}
}
