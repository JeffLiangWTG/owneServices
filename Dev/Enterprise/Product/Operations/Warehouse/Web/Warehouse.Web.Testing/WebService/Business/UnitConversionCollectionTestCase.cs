using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class UnitConversionCollectionTestCase : TestCaseWithFactory
	{
		#region Test Cases

		public void TestAdditionalContructors()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			helper.CreateProductUnit(data.Part1, "BOX", 0.5); // invalid conversion, which will produce warning
			helper.CreateProductUnit(data.Part1, "PLT", 4);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = order.Lines.AddNew();
			line.WE_TransactionQuantity = 1;
			var pick = helper.CreatePickNew(order);

			var unitConversionCollection = new UnitConversionCollection(data.Part1);
			AssertEquals(data.Part1.PK, unitConversionCollection.ProductPK);
			AssertEquals("Only two conversion should be added", 2, unitConversionCollection.Conversions.Count);
			AssertEquals("UNT", unitConversionCollection.Conversions[0].PackType);
			AssertEquals(1m, unitConversionCollection.Conversions[0].Qty);
			AssertEquals("PLT", unitConversionCollection.Conversions[1].PackType);
			AssertEquals(4m, unitConversionCollection.Conversions[1].Qty);
		}

		public void TestProductPK()
		{
			var unitConversions = new UnitConversionCollection();
			AssertEquals(Guid.Empty, unitConversions.ProductPK);

			unitConversions.ProductPK = new Guid("752E77C2-A2B4-4915-8376-53F4B5B40E54");
			AssertEquals(new Guid("752E77C2-A2B4-4915-8376-53F4B5B40E54"), unitConversions.ProductPK);
		}

		#endregion
	}
}
