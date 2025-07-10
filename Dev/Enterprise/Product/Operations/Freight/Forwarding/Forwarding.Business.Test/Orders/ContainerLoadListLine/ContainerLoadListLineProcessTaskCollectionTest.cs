using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLineProcessTaskCollection))]
	sealed class ContainerLoadListLineProcessTaskCollectionTest
		: ProcessTaskCollectionTest<ContainerLoadListLineProcessTaskCollection>
	{
		#region Implementation

		protected override ContainerLoadListLineProcessTaskCollection GetCollectionToTestCore()
		{
			return new ContainerLoadListLineProcessTaskCollection(Factory.NewWithValidTestData<ContainerLoadListLine>());
		}

		#endregion
	}
}
