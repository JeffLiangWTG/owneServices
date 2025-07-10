using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentContainerPenalty))]
	sealed class ShipmentContainerPenaltyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRelatedContainersList()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();
			var penalty = shipment.DeliveryPenalties.AddNew();
			AssertEquals(0, penalty.RelatedContainersList.Count);

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.Containers.AddNew();
			consol.Containers.AddNew();
			shipment.Consols.Add(consol);

			shipment.DeliveryPenalties.DeleteAll();
			Factory.Save();
			penalty = shipment.DeliveryPenalties.AddNew();
			AssertEquals(0, penalty.RelatedContainersList.Count);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(consol.Containers[0]);
			shipment.DeliveryPenalties.DeleteAll();
			Factory.Save();
			penalty = shipment.DeliveryPenalties.AddNew();
			AssertEquals(1, penalty.RelatedContainersList.Count);
		}
	}
}
