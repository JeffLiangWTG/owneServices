using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadPlanCollection))]
	sealed class ContainerLoadPlanCollectionTest : ActiveBusinessObjectCollectionTestCase<ContainerLoadPlanCollection>
	{
	}
}
