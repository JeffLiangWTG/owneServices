using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(RoutesCollection))]
	public class RoutesCollectionTests : NonPersistentBusinessObjectCollectionTestCase<RoutesCollection>
	{
		protected override RoutesCollection GetCollectionToTest()
		{
			return new RoutesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Route(Factory);
		}
	}
}
