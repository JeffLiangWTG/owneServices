using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackLineManyToManyCollection))]
	sealed class ForwardingPackLineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			return container.PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingShipment>().OuterPackLines.AddNew();
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(ForwardingPackLineManyToManyCollection), GetCollectionToTest().GetType());
		}
	}
}
