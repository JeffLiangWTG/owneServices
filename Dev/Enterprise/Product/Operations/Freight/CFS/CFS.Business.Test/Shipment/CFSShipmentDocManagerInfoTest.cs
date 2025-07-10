using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipmentDocManagerInfo))]
	public class CFSShipmentDocManagerInfoTest : ShipmentDocManagerInfoTest
	{
		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.PickupConfirms.DeleteAll();
			shipment.OuterPackLines.AddNew();

			pickupConfirm = shipment.PickupConfirms.AddNew();
			deliveryConfirm = shipment.DeliveryConfirms.AddNew();

			destinationCFSArrivalsConfirm = shipment.DestinationCFSArrivals.AddNew();
			destinationCFSDeparturesConfirm = shipment.DestinationCFSDepartures.AddNew();

			originCFSArrivalsConfirm = shipment.OriginCFSArrivals.AddNew();
			originCFSDeparturesConfirm = shipment.OriginCFSDepartures.AddNew();

			Factory.Save();

			return shipment;
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			var shipment = Factory.New<CFSShipment>();
			return shipment;
		}

		public override void TestAllRelatedObjectsRetrieved()
		{
			var shipment = (CFSShipment)GetPopulatedParentBusinessObject();

			var expectedObjects = new BusinessObject[]
			{
				pickupConfirm,
				deliveryConfirm,
				destinationCFSArrivalsConfirm,
				destinationCFSDeparturesConfirm,
				originCFSArrivalsConfirm,
				originCFSDeparturesConfirm
			};

			var relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertContainsExactElementsInAnyOrder("Should have these confirms in the related business objects ", expectedObjects, relatedObjects);
		}

		CommonPickupDeliveryConfirm pickupConfirm;
		CommonPickupDeliveryConfirm deliveryConfirm;
		CommonPickupDeliveryConfirm destinationCFSArrivalsConfirm;
		CommonPickupDeliveryConfirm destinationCFSDeparturesConfirm;
		CommonPickupDeliveryConfirm originCFSArrivalsConfirm;
		CommonPickupDeliveryConfirm originCFSDeparturesConfirm;
	}
}
