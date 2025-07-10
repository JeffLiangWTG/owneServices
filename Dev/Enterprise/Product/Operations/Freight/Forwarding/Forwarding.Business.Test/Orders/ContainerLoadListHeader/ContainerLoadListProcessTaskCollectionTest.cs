using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListProcessTaskCollection))]
	sealed class ContainerLoadListProcessTaskCollectionTest : ProcessTaskCollectionTest<ContainerLoadListProcessTaskCollection>
	{
		#region Implementation

		protected override ContainerLoadListProcessTaskCollection GetCollectionToTestCore()
		{
			return new ContainerLoadListProcessTaskCollection(Factory.NewWithValidTestData<CYContainerLoadList>());
		}

		#endregion
	}
}
