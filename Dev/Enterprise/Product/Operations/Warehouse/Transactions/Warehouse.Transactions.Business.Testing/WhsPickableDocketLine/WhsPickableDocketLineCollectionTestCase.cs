namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsPickableDocketLineCollectionTestCase<T> : WhsDocketLineCollectionTestCase<T> where T : WhsPickableDocketLineCollection
	{
		#region TestFindLine

		public void TestFindLine()
		{
			WhsPickableDocketLine dodgeLine = Collection.AddNew();
			WhsPickableDocketLine line1 = Collection.AddNew();
			WhsPickableDocketLine line2 = Collection.AddNew();

			dodgeLine.WE_LineNo = 0;
			line1.WE_LineNo = 1;
			line2.WE_LineNo = 2;

			AssertNull(Collection.FindLine(0));
			AssertEquals(line1, Collection.FindLine(1));
			AssertEquals(line2, Collection.FindLine(2));
			AssertNull(Collection.FindLine(3));
		}

		#endregion

		#region TestLocationSortedProperly

		protected override void TestLocationSortedProperlyCore(string locationPropertyToCompare)
		{
			// pickable docket lines do not have location
			Assert(true);
		}

		protected override void TestTransferFromLocationStringSortedProperlyCore(string locationPropertyToCompare)
		{
			// pickable docket lines do not have transfer from location
			Assert(true);
		}

		public void TestSortingByStagingLocationBOM()
		{
			TestSortingByStagingLocationBOMCore();
		}

		protected abstract void TestSortingByStagingLocationBOMCore();

		#endregion

		#region Implementation

		protected new WhsPickableDocketLineCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
