using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(MovementHeaderWrapperCollection))]
	sealed class MovementHeaderWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MovementHeaderWrapperCollection>
	{
		public void TestDefaultInBondNumber()
		{
			var collection = GetCollectionToTest();
			AssertEquals(1, collection.Count);
			var bo1 = collection[0];
			var bo2 = collection.AddNew();
			var bo3 = collection.AddNew();
			AssertEquals(bo1.InBondNumber, "NOT YET SPECIFIED 1");
			AssertEquals(bo2.InBondNumber, "NOT YET SPECIFIED 2");
			AssertEquals(bo3.InBondNumber, "NOT YET SPECIFIED 3");

			collection.Remove(bo1);
			AssertEquals(bo2.InBondNumber, "NOT YET SPECIFIED 1");
			AssertEquals(bo3.InBondNumber, "NOT YET SPECIFIED 2");

			collection.Remove(bo3);
			AssertEquals(bo2.InBondNumber, "NOT YET SPECIFIED 1");
		}

		protected override MovementHeaderWrapperCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipmentsWithoutInBond = new CusInBondShipmentWrapperCollection(consol);
			return new MovementHeaderWrapperCollection(Factory, shipmentsWithoutInBond);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MovementHeaderWrapper(Factory);
		}
	}
}
