using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(ConsignmentJobServiceDependentCollection))]
	sealed class ConsignmentJobServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ConsignmentJobServiceDependentCollection(Factory.New<DtbConsignment>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ConsignmentJobService>();
		}
	}
}
