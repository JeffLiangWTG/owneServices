using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryPackageDetail))]
	public class WhsInventoryPackageDetailTest : NonPersistentBusinessObjectTestCase
	{
		#region Schema

		public void TestSchema()
		{
			AssertEquals("PackageID", WhsInventoryPackageDetail.Schema.PackageID);
			AssertEquals("IsTote", WhsInventoryPackageDetail.Schema.IsTote);
			AssertEquals("OrderNumber", WhsInventoryPackageDetail.Schema.OrderNumber);
		}

		#endregion

		#region TestPackageID

		public void TestPackageID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1001m);
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order1.WD_ExternalReference = "O1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order2.WD_ExternalReference = "O2";

			var packageDetail1 = new WhsInventoryPackageDetail(order1, "PackageID001", false);
			var packageDetail2 = new WhsInventoryPackageDetail(order2, "PackageID002", false);

			AssertEquals("PackageID", "PackageID001", packageDetail1.PackageID);
			AssertEquals("PackageID", "PackageID002", packageDetail2.PackageID);
		}

		#endregion

		#region TestIsTote

		public void TestIsTote()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1001m);
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order1.WD_ExternalReference = "O1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order2.WD_ExternalReference = "O2";

			var packageDetail1 = new WhsInventoryPackageDetail(order1, "PackageID001", false);
			var packageDetail2 = new WhsInventoryPackageDetail(order2, "PackageID002", false);

			AssertEquals("IsTote", false, packageDetail1.IsTote);
			AssertEquals("IsTote", false, packageDetail2.IsTote);
		}

		#endregion

		#region TestOrderNumber

		public void TestOrderNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1001m);
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order1.WD_ExternalReference = "O1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order2.WD_ExternalReference = "O2";

			var packageDetail1 = new WhsInventoryPackageDetail(order1, "PackageID001", false);
			var packageDetail2 = new WhsInventoryPackageDetail(order2, "PackageID002", false);

			AssertEquals("OrderNumber", "O1", packageDetail1.OrderNumber);
			AssertEquals("OrderNumber", "O2", packageDetail2.OrderNumber);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsInventoryPackageDetail();
		}

		#endregion

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
