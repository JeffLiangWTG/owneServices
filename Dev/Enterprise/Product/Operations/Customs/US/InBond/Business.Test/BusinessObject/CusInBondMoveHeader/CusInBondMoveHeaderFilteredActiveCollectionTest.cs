using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderFilteredActiveCollection))]
	sealed class CusInBondMoveHeaderFilteredActiveCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveHeaderFilteredActiveCollection>
	{
		public void TestMatchesFilterCore()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			header.MovementHeaders.AddNew();
			AssertEquals(2, header.FilteredMovementHeaders.Count);
			header.SelectedMovementHeader = moveHeader1.PK;
			AssertEquals("should have been filtered", 1, header.FilteredMovementHeaders.Count);
			AssertEquals("only 1 should be in the collection", moveHeader1, header.FilteredMovementHeaders[0]);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader;
		}

		protected override CusInBondMoveHeaderFilteredActiveCollection GetCollectionToTest() => Factory.New<CusInBondHeader>().FilteredMovementHeaders;
	}
}
