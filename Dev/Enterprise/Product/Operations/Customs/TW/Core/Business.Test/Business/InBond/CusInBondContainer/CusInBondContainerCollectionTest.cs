using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondContainerCollection))]
	sealed class CusInBondContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondContainerCollection>
	{
		protected override CusInBondContainerCollection GetCollectionToTest()
		{
			var cusInBondMoveDetail = Factory.NewWithValidTestData<CusInBondMoveDetail>();
			return new CusInBondContainerCollection(cusInBondMoveDetail);
		}
	}
}
