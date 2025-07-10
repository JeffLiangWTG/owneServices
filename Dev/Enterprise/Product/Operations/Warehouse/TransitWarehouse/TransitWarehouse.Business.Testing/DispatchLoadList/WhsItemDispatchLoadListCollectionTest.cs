using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListCollection))]
	public class WhsItemDispatchLoadListCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemDispatchLoadListCollection>
	{
		#region TestDispatchLoadListsFromDispatchTransportationUnit

		public void TestDispatchLoadListsFromDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchLoadList1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchLoadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dispatchLoadList1.PK, dispatchTransportationUnit.PK);
			Helper.CreateDispatchDLLDTUPivot(dispatchLoadList2.PK, dispatchTransportationUnit.PK);

			Factory.Save();

			AssertEquals("Find DLLs under DTU", 2, new WhsItemDispatchLoadListCollection(Factory, dispatchTransportationUnit).Count);
			AssertContainsExactElementsInAnyOrder(new[] { dispatchLoadList1, dispatchLoadList2 }, new WhsItemDispatchLoadListCollection(Factory, dispatchTransportationUnit));
		}

		#endregion

		#region Implementation

		protected override WhsItemDispatchLoadListCollection GetCollectionToTest()
		{
			return new WhsItemDispatchLoadListCollection(Factory);
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
