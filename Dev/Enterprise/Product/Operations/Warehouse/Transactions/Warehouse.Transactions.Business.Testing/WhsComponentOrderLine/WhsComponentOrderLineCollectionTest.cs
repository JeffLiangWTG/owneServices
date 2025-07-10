using System.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderLineCollectionTest<T> : WhsPickableDocketLineCollectionTestCase<T>
		where T : WhsComponentOrderLineCollection
	{
		#region TestSortingByStagingLocationBOM

		protected override void TestSortingByStagingLocationBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "Motorbike");
			var bikeEngine = Helper.CreateProduct(data.Org1, "Engine");
			var bikeWheel = Helper.CreateProduct(data.Org1, "Wheel");
			Helper.CreateProductBOM(bike, bikeEngine, 1, "UNT");
			Helper.CreateProductBOM(bike, bikeWheel, 2, "UNT");
			Factory.Save();

			var collection = GetCollectionToTest();
			var workOrder = collection.Docket;
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_DocketSubType = "ASS";

			var workOrderLine1 = collection.AddNew();
			workOrderLine1.WE_OP = bikeEngine.PK;
			workOrderLine1.WE_TransactionQuantity = 2m;
			var workOrderLine2 = collection.AddNew();
			workOrderLine2.WE_OP = bikeWheel.PK;
			workOrderLine2.WE_TransactionQuantity = 4m;

			var productParams1 = workOrderLine1.Product.ParamsByWhsAndClient.AddNew();
			productParams1.W3_OH = data.Org1.PK;
			productParams1.W3_WW = data.Whs1.PK;
			productParams1.W3_WL_StagingLocationBOM = data.Whs1.FindLocation("A-2").PK;
			productParams1.W3_WL_InwardsProcessingStagingLocationBOM = data.Whs1.FindLocation("A-2").PK;

			var productParams2 = workOrderLine2.Product.ParamsByWhsAndClient.AddNew();
			productParams2.W3_OH = data.Org1.PK;
			productParams2.W3_WW = data.Whs1.PK;
			productParams2.W3_WL_StagingLocationBOM = data.Whs1.FindLocation("A-10").PK;
			productParams2.W3_WL_InwardsProcessingStagingLocationBOM = data.Whs1.FindLocation("A-10").PK;

			collection.ApplySort(WhsPickableDocketLineCollection.StagingLocationBOMPropertyName, ListSortDirection.Ascending);
			AssertEquals("A-2", ((WhsComponentOrderLine)collection[0]).StagingLocationBOM.ToLocationString());
			AssertEquals("A-10", ((WhsComponentOrderLine)collection[1]).StagingLocationBOM.ToLocationString());

			collection.ApplySort(WhsPickableDocketLineCollection.StagingLocationBOMPropertyName, ListSortDirection.Descending);
			AssertEquals("A-10", ((WhsComponentOrderLine)collection[0]).StagingLocationBOM.ToLocationString());
			AssertEquals("A-2", ((WhsComponentOrderLine)collection[1]).StagingLocationBOM.ToLocationString());
		}

		#endregion
	}
}
