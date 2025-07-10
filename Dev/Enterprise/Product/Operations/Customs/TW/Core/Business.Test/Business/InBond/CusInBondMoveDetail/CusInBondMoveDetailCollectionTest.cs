using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailCollection))]
	sealed class CusInBondMoveDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveDetailCollection>
	{
		protected override CusInBondMoveDetailCollection GetCollectionToTest()
		{
			var cusInBondMoveHeader = Factory.NewWithValidTestData<CusInBondMoveHeader>();
			return new CusInBondMoveDetailCollection(cusInBondMoveHeader);
		}
	}
}
