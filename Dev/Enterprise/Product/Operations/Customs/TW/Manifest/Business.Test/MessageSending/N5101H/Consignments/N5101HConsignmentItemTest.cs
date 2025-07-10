using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsignmentItemTest : TestCaseWithFactory
	{
		public void TestUCRId()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_UCRNumber = "AWG";
			IN5101HConsignmentItem consignmentItem = new N5101HConsignmentItem(bill);
			AssertEquals("AWG", consignmentItem.UCRId);
		}

		public void TestSplit()
		{
			CombineAssertions(() =>
			{
				var bill = Factory.NewWithValidTestData<AsycudaBill>();
				bill.ABL_SplitQuantity = 1;
				IN5101HConsignmentItem consignmentItem = new N5101HConsignmentItem(bill);
				AssertEquals("Y", consignmentItem.Split);

				bill.ABL_SplitQuantity = 0;
				consignmentItem = new N5101HConsignmentItem(bill);
				AssertEquals(ZString.Empty, consignmentItem.Split);
			});
		}

		public void TestTotalPackageQuantity()
		{
			CombineAssertions(() =>
			{
				var bill = Factory.NewWithValidTestData<AsycudaBill>();
				bill.ABL_SplitQuantity = 1;
				var consignmentItem = new N5101HConsignmentItem(bill);
				AssertEquals(1m, consignmentItem.TotalPackageQuantity);

				bill.ABL_SplitQuantity = 0;
				consignmentItem = new N5101HConsignmentItem(bill);
				AssertEquals(0m, consignmentItem.TotalPackageQuantity);
			});
		}

		public void TestGoodsMeasure()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			IN5101HConsignmentItem consignmentItem = new N5101HConsignmentItem(bill);
			AssertType<N5101HGoodsMeasure>(consignmentItem.GoodsMeasure);
		}
	}
}
