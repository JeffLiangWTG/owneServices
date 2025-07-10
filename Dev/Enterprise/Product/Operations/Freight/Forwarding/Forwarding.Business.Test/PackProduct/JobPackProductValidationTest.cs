using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JobPackProductValidationTest : BusinessObjectValidationTestCase
	{
		public void TestD2_ProductQuantity()
		{
			PackProduct.D2_ProductQuantity = 0.5m;
			Assert("0.5 should be valid.", !PackProduct.D2_ProductQuantityInfo.HasNotifications());

			PackProduct.D2_ProductQuantity = 0;
			Assert("0 should be valid.", !PackProduct.D2_ProductQuantityInfo.HasNotifications());

			PackProduct.D2_ProductQuantity = -1;
			Assert("Negative value should be error with no warning.", PackProduct.D2_ProductQuantityInfo.HasErrors() && !PackProduct.D2_ProductQuantityInfo.HasWarnings());
		}

		public void TestD2_ProductUnitOfQty()
		{
			PackProduct.D2_ProductUnitOfQty = "";
			AssertEquals(false, PackProduct.D2_ProductUnitOfQtyInfo.HasErrors());

			PackProduct.D2_ProductQuantity = 10m;
			PackProduct.D2_ProductUnitOfQty = "";
			AssertEquals("Error if quantity is specified and unit of quantity is not specified", true, PackProduct.D2_ProductUnitOfQtyInfo.HasErrors());

			PackProduct.D2_ProductUnitOfQty = "XXX";
			AssertEquals(true, PackProduct.D2_ProductUnitOfQtyInfo.HasErrors());

			PackProduct.D2_ProductUnitOfQty = "BOX";
			AssertEquals("Correct unit of quantity, no errors", false, PackProduct.D2_ProductUnitOfQtyInfo.HasErrors());

			PackProduct.D2_ProductUnitOfQty = "BAG";
			AssertEquals("Correct unit of quantity, no errors", false, PackProduct.D2_ProductUnitOfQtyInfo.HasErrors());
		}

		#region Implementation

		PackProduct PackProduct;

		protected override void SetUp()
		{
			ForwardingPackLine packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct = packline.Products.AddNew();
		}

		#endregion
	}
}
