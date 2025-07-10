namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsConcurrencyTestCase : WhsTestCaseWithFactory
	{
		//public void TestDespatch()
		//{
		//    WhsWarehouse Whs = Helper.CreateWarehouse("1", "A", 2, 2);
		//    OrgHeader Org = Helper.CreateClient();
		//    OrgSupplierPart Prod = Helper.CreateProduct(Org, "P1");

		//    WhsInwards Inwards = Helper.CreateWhsInwards(Org, Whs, Helper.Notify);
		//    Helper.CreateWhsInwardsInventoryLine(Inwards, Prod, 100m);
		//    Inwards.AllocateLocationsWithMock();
		//    Inwards.FinaliseDocket();
		//    AssertIsFinalisedPrecondition(Inwards);

		//    WhsOrder Order = Helper.CreateWhsOrder(Org, Whs, Helper.Notify);
		//    Helper.CreateWhsOrderLine(Order, Prod, 40m);

		//    WhsPick Pick = Helper.CreatePick(Order);
		//    AssertEquals("Precondition: Order should be picked", true, Order.IsPicking);
		//    Factory.Save();

		//    BusinessObjectFactory FactoryA = new BusinessObjectFactory();
		//    BusinessObjectFactory FactoryB = new BusinessObjectFactory();
		//    FactoryA.RefreshEnabled = false;
		//    FactoryB.RefreshEnabled = false;

		//    WhsPick PickA = FactoryA.Load<WhsPick>(Pick.PK);
		//    WhsPick PickB = FactoryB.Load<WhsPick>(Pick.PK);

		//    //PickA.WP_PickStatusInfo.ConcurrencyPolicy = ConcurrencyPolicy.Ignore;
		//    //PickB.WP_PickStatusInfo.ConcurrencyPolicy = ConcurrencyPolicy.Ignore;

		//    PickB.FinaliseAllOrders();
		//    PickB.FinalisePick();
		//    FactoryB.Save();

		//    PickA.FinaliseAllOrders();
		//    PickA.FinalisePick();
		//    FactoryA.Save();

		//    CompareTransactionsToStockOnHand Compare = new CompareTransactionsToStockOnHand();
		//    AssertEquals("Should be no imbalances", 0, Compare.Run());
		//}
	}
}
