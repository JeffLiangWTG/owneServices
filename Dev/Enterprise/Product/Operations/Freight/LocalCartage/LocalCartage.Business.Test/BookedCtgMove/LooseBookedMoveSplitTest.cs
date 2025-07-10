using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.BookedCtgMove.Testing
{
	[TestedType(typeof(LooseBookedMoveSplit))]
	internal class LooseBookedMoveSplitTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			return new LooseBookedMoveSplit(master);
		}
	}
}
