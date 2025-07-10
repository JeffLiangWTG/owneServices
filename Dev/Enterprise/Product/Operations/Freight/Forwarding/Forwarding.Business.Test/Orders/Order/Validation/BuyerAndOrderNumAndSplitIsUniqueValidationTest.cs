using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class BuyerAndOrderNumAndSplitIsUniqueValidationTest : TestCaseWithFactory
	{
		public void TestCheckBuyerAndOrderNumAndSplitIsUnique()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order1 = Factory.NewWithValidTestData<Order>();
			BuyerAndOrderNumAndSplitIsUniqueValidation validation1 = new BuyerAndOrderNumAndSplitIsUniqueValidation(order1);

			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "1";
			order1.JD_OrderNumberSplit = (ZByte)0;
			order1.Validation.ValidateJD_OrderNumber();

			Factory.Save();
			AssertNoErrors("Should have no errors on OrderNumber", order1.JD_OrderNumberInfo);

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "1";
			order2.JD_OrderNumberSplit = (ZByte)0;

			BuyerAndOrderNumAndSplitIsUniqueValidation validation2 = new BuyerAndOrderNumAndSplitIsUniqueValidation(order2);
			order2.Validation.ValidateJD_OrderNumber();
			AssertHasErrors("Should have errors on OrderNumber", order2.JD_OrderNumberInfo);

			order2.JD_OrderNumber = "2";
			order2.Validation.ValidateJD_OrderNumber();
			AssertNoErrors("Should have no errors on OrderNumber since now different", order2.JD_OrderNumberInfo);

			order1.JD_IsCancelled = true;
			Factory.Save();

			order2.JD_OrderNumber = "1";
			order2.Validation.ValidateJD_OrderNumber();
			AssertNoErrors("Should have no errors on OrderNumber if Order1 is cancelled", order2.JD_OrderNumberInfo);
		}
	}
}
