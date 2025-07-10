using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(CartonGroupParentOrgMiscServCollection))]
	class CartonGroupParentOrgMiscServCollectionTestCase : WhsActiveBusinessObjectCollectionTestCase<CartonGroupParentOrgMiscServCollection>
	{
		#region TestCollectionUsingMaster

		public void TestCollectionUsingMaster()
		{
			var group1 = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var group2 = Helper.CreateWhsCartonGroup("G2", "Group 2");
			var org1 = Helper.CreateClient("1", "1");
			var org2 = Helper.CreateClient("2", "2");
			Factory.Save();

			var group1_Collection = new CartonGroupParentOrgMiscServCollection(group1);
			var group2_Collection = new CartonGroupParentOrgMiscServCollection(group2);
			AssertEquals("Precondition", 0, group1_Collection.Count);
			AssertEquals("Precondition", 0, group2_Collection.Count);

			group1_Collection.Add(org1.MiscServ);
			AssertEquals("Should have added to Group1.", 1, group1_Collection.Count);
			AssertEquals(group1.PK, org1.MiscServ.OM_WCG_CartonGroup);
			AssertEquals(0, group2_Collection.Count);

			group2_Collection.Add(org1.MiscServ);
			AssertEquals(0, group1_Collection.Count);
			AssertEquals(1, group2_Collection.Count);

			group2_Collection.Add(org2.MiscServ);
			AssertEquals(0, group1_Collection.Count);
			AssertEquals(2, group2_Collection.Count);
			AssertNoExceptionThrown(Factory.Save);

			group2_Collection.RemoveFromRelationship(org2.MiscServ);
			AssertEquals(ZGuid.Empty, org2.MiscServ.OM_WCG_CartonGroup);
		}

		#endregion

		#region Implementation

		protected new WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#region GetCollectionToTest

		protected override CartonGroupParentOrgMiscServCollection GetCollectionToTest()
		{
			return new CartonGroupParentOrgMiscServCollection(Helper.CreateWhsCartonGroup("1", "1"));
		}

		#endregion

		#endregion
	}
}
