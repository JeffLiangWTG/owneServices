using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSCusInBondMoveHeaderCollection))]
	public class SPTSCusInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<SPTSCusInBondMoveHeaderCollection>
	{
		protected override SPTSCusInBondMoveHeaderCollection GetCollectionToTest()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			return new SPTSCusInBondMoveHeaderCollection(sptsHeader);
		}
	}
}
