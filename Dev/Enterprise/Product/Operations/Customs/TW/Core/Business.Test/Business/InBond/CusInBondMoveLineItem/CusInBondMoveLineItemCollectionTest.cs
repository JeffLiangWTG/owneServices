using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveLineItemCollection))]
	sealed class CusInBondMoveLineItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveLineItemCollection>
	{
		protected override CusInBondMoveLineItemCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return new CusInBondMoveLineItemCollection(moveDetail);
		}
	}
}
