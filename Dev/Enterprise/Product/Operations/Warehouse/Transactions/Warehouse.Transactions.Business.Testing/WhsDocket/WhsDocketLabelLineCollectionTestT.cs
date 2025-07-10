using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLabelLineCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : WhsDocketLabelLineCollection
	{
		public void TestProperties()
		{
			WhsDocketLabelLineCollection collection = GetCollectionToTest();
			AssertEquals("Empty collection", 0, collection.Count);
			AssertEquals("Cannot add lines", false, collection.AllowNew);
			AssertEquals("Cannot delete lines", false, collection.AllowRemove);
			AssertEquals("Default total number of labels", 0, collection.TotalNumberOfLabels);
			AssertEquals("Default number of labels to print", 0, collection.NumberOfLabelsToPrint);
		}

		public void TestUnqiueness()
		{
			WhsDocketLabelLineCollection lines = GetCollectionToTest();
			AssertEquals("Empty collection", 0, lines.Count);

			WhsDocketLabelLine line = (WhsDocketLabelLine)GetNewElementToAddToTheCollection();
			lines.Add(line);
			AssertEquals("1 line", 1, lines.Count);
			AssertEquals("First element is correct", line, lines[0]);

			lines.Add(line);
			AssertEquals("Still only 1 line", 1, lines.Count);
			AssertEquals("First element is correct", line, lines[0]);

			WhsOrder order2 = Factory.NewWithValidTestData<WhsOrder>();
			WhsDocketLabelLine line2 = new WhsDocketLabelLine(order2, 3, 3);
			lines.Add(line2);
			AssertEquals("2 lines", 2, lines.Count);
			AssertEquals("Second element is correct", line2, lines[1]);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsDocketLabelLine(Factory.NewWithValidTestData<WhsOrder>(), 10, 10);
		}

		public abstract void TestCollection();
	}
}
