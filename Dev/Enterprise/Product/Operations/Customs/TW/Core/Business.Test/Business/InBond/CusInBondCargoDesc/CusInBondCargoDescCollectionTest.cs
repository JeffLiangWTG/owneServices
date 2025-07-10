using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDescCollection))]
	sealed class CusInBondCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondCargoDescCollection>
	{
		protected override CusInBondCargoDescCollection GetCollectionToTest()
		{
			var container = Factory.New<CusInBondContainer>();
			return new CusInBondCargoDescCollection(container);
		}
	}
}
