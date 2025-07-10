using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadPlanProcessTaskCollection))]
	sealed class ContainerLoadPlanProcessTaskCollectionTest : ProcessTaskCollectionTest<ContainerLoadPlanProcessTaskCollection>
	{
		#region Implementation

		protected override ContainerLoadPlanProcessTaskCollection GetCollectionToTestCore()
		{
			return new ContainerLoadPlanProcessTaskCollection(Factory.NewWithValidTestData<CFSContainerLoadList>());
		}

		#endregion
	}
}
