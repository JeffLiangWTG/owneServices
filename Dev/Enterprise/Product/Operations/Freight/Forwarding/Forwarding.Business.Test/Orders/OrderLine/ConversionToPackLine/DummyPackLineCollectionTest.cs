using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(DummyPackLineCollection))]
	sealed class DummyPackLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DummyPackLineCollection>
	{
		#region Implementation

		protected override DummyPackLineCollection GetCollectionToTest()
		{
			return new DummyPackLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyPackLine(Factory);
		}

		#endregion
	}
}
