using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickLineCollectionND))]
	class WhsPickLineCollectionNDTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, PickLines.AllowNew);
		}

		#endregion

		#region TestAddPickLinesWithRemovalFromOriginalCollection_PickableDocketLine

		public void TestAddPickLinesWithRemovalFromOriginalCollection_PickableDocketLine()
		{
			var pickableDocket = Factory.New<WhsOrder>();
			var pickableDocketLine1 = pickableDocket.Lines.AddNew();
			var pickableDocketLine2 = pickableDocket.Lines.AddNew();

			var allPickLines = new WhsPickLineCollectionND(Factory);
			WhsPickLine pickLine11 = allPickLines.AddNew();
			WhsPickLine pickLine12 = allPickLines.AddNew();
			WhsPickLine pickLine21 = allPickLines.AddNew();
			WhsPickLine pickLine22 = allPickLines.AddNew();

			pickLine11.WZ_WE_TransactionLine = pickableDocketLine1.PK;
			pickLine12.WZ_WE_TransactionLine = pickableDocketLine1.PK;
			pickLine21.WZ_WE_TransactionLine = pickableDocketLine2.PK;
			pickLine22.WZ_WE_TransactionLine = pickableDocketLine2.PK;

			AssertEquals("Precondition collection should be empty", 0, PickLines.Count);

			PickLines.AddPickLinesWithRemovalFromOriginalCollection(pickableDocketLine1, allPickLines);
			AssertEquals("2 lines should have been Added", 2, PickLines.Count);
			AssertCollectionContains("Incorrect Pick Line was added", pickLine11, PickLines);
			AssertCollectionContains("Incorrect Pick Line was added", pickLine12, PickLines);

			AssertEquals("Added lines should have been Removed from original collection", 2, allPickLines.Count);
			AssertCollectionNotContains("Incorrect Pick Line was removed", pickLine11, allPickLines);
			AssertCollectionNotContains("Incorrect Pick Line was removed", pickLine12, allPickLines);
		}

		#endregion

		#region TestGetQtyPicked

		public void TestGetQtyPicked()
		{
			var pickLines = new WhsPickLineCollectionND(Factory);
			AssertEquals(0m, pickLines.GetQtyPicked());

			pickLines.AddNew().WZ_Units = 9;
			pickLines.AddNew().WZ_Units = 6;
			pickLines.AddNew().WZ_Units = 10;
			AssertEquals(25m, pickLines.GetQtyPicked());
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsPickLineCollectionND(Factory, new ZQuery());
		}

		WhsPickLineCollectionND pickLines;
		protected WhsPickLineCollectionND PickLines
		{
			get { return pickLines ?? (pickLines = (WhsPickLineCollectionND)GetCollectionToTest()); }
		}

		#endregion
	}
}
