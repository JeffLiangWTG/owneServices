using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(CartonsCollection))]
	public class CartonsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CartonsCollection>
	{
		protected override CartonsCollection GetCollectionToTest()
		{
			return new CartonsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyCartonDefinition();
		}
	}
}
