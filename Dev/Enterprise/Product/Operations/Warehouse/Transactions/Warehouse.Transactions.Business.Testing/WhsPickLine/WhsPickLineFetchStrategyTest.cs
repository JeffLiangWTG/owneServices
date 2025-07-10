using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickLineFetchStrategyTest : WhsTestCaseWithFactory
	{
		#region TestDocketLineFetchHintAddedOnlyWhenNecessary

		public void TestDocketLineFetchHintAddedOnlyWhenNecessary()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var reservedInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var reservedOrderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var nonReservedOrderLine = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var reservedPickLine = reservedOrderLine.ReserveStockIfAbleTo(reservedInventory);
			AssertNotNull("Precondition - Stock is reserved.", reservedPickLine);

			Factory.Save();

			var otherFactory1 = new BusinessObjectFactory();
			otherFactory1.Load<WhsPickLine>(reservedPickLine.PK);
			AssertEquals("Should *not* add the WhsDocketLine FetchHints as only the reserved pick line has been loaded.", 0, otherFactory1.ActiveFetchHintsForTable(WhsDocketLineSchema.Constants.TableName));

			var otherFactory2 = new BusinessObjectFactory();
			otherFactory2.Load<WhsDocketLine>(reservedOrderLine.PK);
			otherFactory2.Load<WhsPickLine>(reservedPickLine.PK);
			AssertEquals("Should *not* add the WhsDocketLine FetchHint as the corresponding Docket Line had already been loaded.", 0, otherFactory2.ActiveFetchHintsForTable(WhsDocketLineSchema.Constants.TableName));

			var otherFactory3 = new BusinessObjectFactory();
			otherFactory3.Load<WhsDocketLine>(nonReservedOrderLine.PK);
			otherFactory3.Load<WhsPickLine>(reservedPickLine.PK);
			AssertEquals("Should *not* add the WhsDocketLine FetchHints as the corresponding Docket Line has not been loaded.", 0, otherFactory3.ActiveFetchHintsForTable(WhsDocketLineSchema.Constants.TableName));
		}

		#endregion
	}
}
