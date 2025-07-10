using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderCollection))]
	sealed class CusInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveHeaderCollection>
	{
		protected override CusInBondMoveHeaderCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			return new CusInBondMoveHeaderCollection(header);
		}
	}
}
