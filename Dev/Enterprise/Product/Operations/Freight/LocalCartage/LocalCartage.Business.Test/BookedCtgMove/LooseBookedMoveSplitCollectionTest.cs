using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.BookedCtgMove.Testing
{
	[TestedType(typeof(LooseBookedMoveSplitCollection))]
	public class LooseBookedMoveSplitCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LooseBookedMoveSplitCollection>
	{
		protected override LooseBookedMoveSplitCollection GetCollectionToTest()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			return new LooseBookedMoveSplitCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			return new LooseBookedMoveSplit(master);
		}
	}
}
