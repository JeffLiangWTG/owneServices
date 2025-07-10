using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	public class WhsBondedWarehouseTransactionLineCollectionTest : WhsWarehouseTransactionLineCollectionTest
	{
		public override void TestConstructor()
		{
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line3 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine[]
				lines = new WhsBondedWarehouseTransactionLine[3] { line1, line2, line3 };
			WhsBondedWarehouseTransactionLineCollection collection = new WhsBondedWarehouseTransactionLineCollection();
			AssertEquals(0, collection.Count);
			collection = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			AssertEquals(3, collection.Count);
			collection = WhsBondedWarehouseTransactionLineCollection.GetNew(line1);
			AssertEquals(1, collection.Count);
			collection = WhsBondedWarehouseTransactionLineCollection.GetNew(lines);
			AssertEquals(3, collection.Count);
		}

		public override void TestIndexer()
		{
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line3 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLineCollection collection =
				WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2);
			AssertEquals(line1, collection[0]);
			AssertEquals(line2, collection[1]);
			collection[0] = line3;
			AssertEquals(line3, collection[0]);
			AssertEquals(false, collection.Contains(line1));
			AssertEquals(true, collection.Contains(line2));
			AssertEquals(true, collection.Contains(line3));
		}

		public override void TestAdd()
		{
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLineCollection collection = new WhsBondedWarehouseTransactionLineCollection();
			AssertEquals(0, collection.Count);
			collection.Add(line1);
			AssertEquals(1, collection.Count);
			collection.Add(line2);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(line1));
			AssertEquals(true, collection.Contains(line2));
		}

		public override void TestClone()
		{
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLineCollection collection =
				WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2);
			WhsBondedWarehouseTransactionLineCollection collection1 = collection.Clone();
			AssertEquals(2, collection1.Count);
			AssertEquals(true, collection1.Contains(line1));
			AssertEquals(true, collection1.Contains(line2));
		}
	}
}
