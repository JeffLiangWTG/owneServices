using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderDocManagerInfo))]
	class WhsVASOrderDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var serviceArea = Helper.CreateServiceAreaForVASOrder(Data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, Data.Org1);
			var whsVASOrderDocManagerInfo = new WhsVASOrderDocManagerInfo(vasOrder);

			AssertNotNull(whsVASOrderDocManagerInfo);

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { VASOrder.Client }, whsVASOrderDocManagerInfo.RelatedObjects);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var serviceArea = Helper.CreateServiceAreaForVASOrder(Data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, Data.Org1);
			var docManagerInfo = vasOrder.DocManagerInfo();
			AssertNotNull(docManagerInfo);
			var docManagerInfoSupportReadonly = docManagerInfo as ISupportReadOnlyOverride;
			AssertNotNull(docManagerInfoSupportReadonly);

			docManagerInfoSupportReadonly.SetReadOnly(true);
			AssertEquals("Should be equal", true, docManagerInfo.ReadOnly);

			docManagerInfoSupportReadonly.SetReadOnly(false);
			AssertEquals("Should be equal", false, docManagerInfo.ReadOnly);
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		TestDataSimpleEnvironment Data => data ?? (data = new TestDataSimpleEnvironment(Factory));
		TestDataSimpleEnvironment data;

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<WhsVASOrder>();

		public override BusinessObject GetPopulatedParentBusinessObject() => VASOrder;

		WhsVASOrder VASOrder => vasOrder ?? (vasOrder = Helper.CreateWhsVASOrder(ServiceArea, data.Org1));
		WhsVASOrder vasOrder;

		WhsArea ServiceArea => serviceArea ?? (serviceArea = Helper.CreateServiceAreaForVASOrder(Data.Whs1));
		WhsArea serviceArea;

		#endregion
	}
}
