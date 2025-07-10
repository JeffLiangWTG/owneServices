using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSMoveDetailCollection))]
	public class SPTSMoveDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<SPTSMoveDetailCollection>
	{
		protected override SPTSMoveDetailCollection GetCollectionToTest()
		{
			var header = Factory.New<SPTSHeader>();
			var moveMentHeader = header.MovementHeader;
			return new SPTSMoveDetailCollection(moveMentHeader);
		}
	}
}
