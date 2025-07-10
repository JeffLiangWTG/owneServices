using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostingCustomsFeeCollection))]
	sealed class LandedCostingCustomsFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LandedCostingCustomsFeeCollection>
	{
		public void TestIndexer()
		{
			LandedCostingCustomsFeeCollection collection = new LandedCostingCustomsFeeCollection(Factory);
			LandedCostingCustomsFee fee = collection.AddNew();
			fee.FeeType = "AAA";
			fee.FeeAmount = 2m;

			LandedCostingCustomsFee fee2 = collection.AddNew();
			fee2.FeeType = "BBB";
			fee2.FeeAmount = 3m;

			AssertEquals("PreCondition", 2, collection.Count);

			AssertEquals(fee, collection["AAA"]);
			AssertEquals(fee2, collection["BBB"]);
		}

		public void TestUpdateAmount()
		{
			LandedCostingCustomsFeeCollection collection = new LandedCostingCustomsFeeCollection(Factory);
			collection.UpdateAmount(new CustomsFee("CCC", 10m));

			AssertEquals(1, collection.Count);
			AssertEquals("CCC", collection[0].FeeType);
			AssertEquals(10m, collection[0].FeeAmount);

			collection.UpdateAmount(new CustomsFee("CCC", 1m));
			AssertEquals(1, collection.Count);
			AssertEquals("CCC", collection[0].FeeType);
			AssertEquals(11m, collection[0].FeeAmount);

			collection.UpdateAmount(new CustomsFee("DDD", 2m));
			AssertEquals(2, collection.Count);
		}

		public void TestFind()
		{
			LandedCostingCustomsFeeCollection collection = new LandedCostingCustomsFeeCollection(Factory);
			LandedCostingCustomsFee fee = collection.AddNew();
			fee.FeeType = "AAA";
			fee.FeeAmount = 2m;

			LandedCostingCustomsFee fee2 = collection.AddNew();
			fee2.FeeType = "BBB";
			fee2.FeeAmount = 3m;

			AssertEquals(fee, collection.Find("AAA"));
			AssertEquals(fee2, collection.Find("BBB"));
		}

		public void TestLoadCollection()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.Fees = new ICustomsFee[] { new CustomsFee("AAA", 2m), new CustomsFee("BBB", 3m) };

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lcHistory = lcHeader.Histories.AddNew();
			lcHistory.UltimateDistributee = distributee;

			LandedCostingCustomsFeeCollection collection = new LandedCostingCustomsFeeCollection(Factory);
			collection.LoadCollection(lcHistory.UltimateDistributee.Fees);
			AssertEquals(2, collection.Count);

			AssertEquals(2m, collection[0].FeeAmount);
			AssertEquals(3m, collection[1].FeeAmount);
		}

		public void TestLoadCollectionWithHistories()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.Fees = new ICustomsFee[] { new CustomsFee("AAA", 1m), new CustomsFee("BBB", 2m) };

			DummyIUltimateDistributee distributee2 = Factory.New<DummyIUltimateDistributee>();
			distributee2.Fees = new ICustomsFee[] { new CustomsFee("BBB", 4m), new CustomsFee("CCC", 16m) };

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lcHistory = lcHeader.Histories.AddNew();
			lcHistory.UltimateDistributee = distributee;

			LandedCostHistory lcHistory2 = lcHeader.Histories.AddNew();
			lcHistory2.UltimateDistributee = distributee2;

			LandedCostingCustomsFeeCollection collection = new LandedCostingCustomsFeeCollection(Factory);
			collection.LoadCollection(lcHeader.Histories);
			AssertEquals(3, collection.Count);

			AssertEquals(1m, collection[0].FeeAmount);
			AssertEquals(6m, collection[1].FeeAmount);
			AssertEquals(16m, collection[2].FeeAmount);
		}

		protected override LandedCostingCustomsFeeCollection GetCollectionToTest() => new LandedCostingCustomsFeeCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new LandedCostingCustomsFee(Factory);
	}
}
