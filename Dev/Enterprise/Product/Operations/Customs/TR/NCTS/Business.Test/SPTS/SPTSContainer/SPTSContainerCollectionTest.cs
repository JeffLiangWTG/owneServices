using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSContainerCollection))]
	public class SPTSContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<SPTSContainerCollection>
	{
		protected override SPTSContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<SPTSHeader>();
			return header.HeaderContainers;
		}
	}
}
