using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(YardUnitMovementCollection))]
	sealed class YardUnitMovementCollectionTest : ActiveBusinessObjectCollectionTestCase<YardUnitMovementCollection>
	{
		#region Implementation

		protected override YardUnitMovementCollection GetCollectionToTest()
		{
			var yardUnit = Factory.NewWithValidTestData<YardUnit>();
			var collection = new YardUnitMovementCollection(yardUnit);

			return collection;
		}

		#endregion
	}
}
