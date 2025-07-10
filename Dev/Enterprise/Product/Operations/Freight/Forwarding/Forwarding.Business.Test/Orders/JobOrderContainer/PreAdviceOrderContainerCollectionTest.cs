using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(PreAdviceOrderContainerCollection))]
	sealed class PreAdviceOrderContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilterCorrectlyLoadsRecords()
		{
			var shipmentPlanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			var collection = new PreAdviceOrderContainerCollection(shipmentPlanning);
			collection.AddNew();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var shipmentPlanningOtherFactory = otherFactory.Load<JobShipmentPreplanning>(shipmentPlanning.PK);

			AssertEquals("The containers should have been loaded correctly.", 1, shipmentPlanningOtherFactory.Containers.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			return new PreAdviceOrderContainerCollection(preAdvice);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderContainer>();
		}
	}
}
