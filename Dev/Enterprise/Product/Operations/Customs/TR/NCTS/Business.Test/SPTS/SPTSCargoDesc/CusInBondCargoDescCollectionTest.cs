using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSCargoDescCollection))]
	public class CusInBondCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<SPTSCargoDescCollection>
	{
		protected override SPTSCargoDescCollection GetCollectionToTest()
		{
			var container = Factory.New<SPTSContainer>();
			return new SPTSCargoDescCollection(container);
		}
	}
}
