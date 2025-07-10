using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLineCollection))]
	sealed class ContainerLoadListLineCollectionTest : ActiveBusinessObjectCollectionTestCase<ContainerLoadListLineCollection>
	{
		protected override ContainerLoadListLineCollection GetCollectionToTest()
		{
			var loadListHeader = Factory.New<CommonContainerLoadList>();
			return new ContainerLoadListLineCollection(loadListHeader);
		}
	}
}
